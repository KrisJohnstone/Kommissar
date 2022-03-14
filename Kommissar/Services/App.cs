using System.Collections.Immutable;
using Kommissar.Model;
using Kommissar.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Kommissar.Services;
public class AppService : IApp
{
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;
    private readonly IKubeRepo _kube;
    private readonly IDbRepository _db;

    public AppService(IOptions<AppSettings> appSettings, ILogger<AppService> logger, IKubeRepo kube, IDbRepository db)
    {
        _logger = logger;
        _kube = kube;
        _db = db;
        _appSettings = appSettings.Value;
    }

    public async ValueTask Run(string[] args)
    {
        var namespaces = await GetEnvList(
                                                    _appSettings.Namespaces.ToImmutableArray());
        if (namespaces.Count == 0)
        {
            _logger.LogCritical("No Namespaces Returned from Cluster");
            return;
        }
        
        // First we need to populate the dataset with current state.
        var conts = new List<ContainerList>();
        namespaces.ForEach(async m =>
        {
            var listConts = new List<Container>();
            
            listConts.AddRange(await GetDeployments(m));
            listConts.AddRange(await GetStatefulSets(m));
            
            conts.Add(new ContainerList()
            {
                Namespace = m,
                Containers = listConts
            });
        });
        
        //now we need to populate the db
        await AddOrUpdate(conts);
    }

    public async ValueTask<List<Container>> GetStatefulSets(string ns)
    {
        var liststs = new List<Container>();
        var sts = await _kube.GetListOfStatefulSets(ns);
        sts.ForEach(i =>
        {
            var images = from c in i.Spec.Template.Spec.Containers select c.Image;
            foreach (var image in images)
            {
                var imageArray = image.Split(new[] {':', '/'}, StringSplitOptions.None);
                var containerName = new string("");
                var version = new string("");
                containerName = imageArray[^2] == ":" ? imageArray[^3] : imageArray[^1];
                version = imageArray[^1] == ":" ? imageArray[^1] : "latest";
                
                liststs.Add(new Container()
                {
                    ContainerName = containerName,
                    FullPath = image,
                    ContainerVersion = version
                });
            }
        });
        return liststs ;
    }
    
    public async ValueTask<List<Container>> GetDeployments(string ns)
    {
        var listdeps = new List<Container>();
        var deps = await _kube.GetListOfDeployments(ns);
        deps.ForEach(i =>
        {
            var images = from c in i.Spec.Template.Spec.Containers select c.Image;
            foreach (var image in images)
            {
                var imageArray = image.Split(new[] {':', '/'}, StringSplitOptions.None);
                var containerName = new string("");
                var version = new string("");
                containerName = imageArray[^2] == ":" ? imageArray[^3] : imageArray[^1];
                version = imageArray[^1] == ":" ? imageArray[^1] : "latest";
                
                listdeps.Add(new Container()
                {
                    ContainerName = containerName,
                    FullPath = image,
                    ContainerVersion = version
                });
            }
        });
        return listdeps;
    }
    
    public async ValueTask AddOrUpdate(List<ContainerList> containers)
    {
        foreach (var container in containers)
        {
            //This is mickey mouse but given we dont need to worry about scaling....
            var documents = await _db.GetDocumentAsync(container.Namespace);
            
            // namespace doesnt exist
            if (documents is null)
            {
                await _db.AddNewList(container);
                continue;
            }

            await _db.UpdateContainers(container);
        }
        
        //namespace exists in db - remove
        await _db.RemoveMissingDocuments(containers);
    }

    public async ValueTask<List<string>> GetEnvList(IEnumerable<string> filter)
    {
        var mbrNamespaces = new List<string>();
        var namespaceList = await _kube.GetListOfEnvs();

        foreach (var s in filter)
        {
            mbrNamespaces.AddRange(from item in namespaceList.Items
                where item.Metadata.Name.Split(new[] {'-'})[0] == s
                select item.Metadata.Name); ;
        }

        return mbrNamespaces.ToList();
    }
}