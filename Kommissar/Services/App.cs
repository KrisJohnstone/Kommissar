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
    private readonly IArtifactory _artifactory;

    public AppService(IOptions<AppSettings> appSettings, ILogger<AppService> logger, IKubeRepo kube, IDbRepository db, IArtifactory artifactory)
    {
        _logger = logger;
        _kube = kube;
        _db = db;
        _artifactory = artifactory;
        _appSettings = appSettings.Value;
    }

    public async ValueTask Run(string[] args)
    {
        var namespaces = await GetEnvList(
                                                    _appSettings.NamespacesPrefix.ToImmutableArray());
        if (namespaces.Count == 0)
        {
            _logger.LogCritical("No Namespaces Returned from Cluster");
            return;
        }
        
        // First we need to populate the dataset with current state.
        var conts = new List<ContainerList>();
        foreach (var ns in namespaces)
        {
            _logger.LogInformation("Currently Getting Resources For: {namespace}", ns);
            var listConts = new List<Container>();
            
            listConts.AddRange(await GetDeployments(ns));
            listConts.AddRange(await GetStatefulSets(ns));

            if (listConts.Count > 0)
            {
                conts.Add(new ContainerList()
                {
                    Namespace = ns,
                    Environment = ns.Substring(ns.LastIndexOf("-", StringComparison.Ordinal)+1),
                    Containers = listConts
                });
            }
        }
        await AddOrUpdate(conts);
    }

    public async ValueTask<List<Container>> GetStatefulSets(string ns)
    {
        var liststs = new List<Container>();
        var sts = await _kube.GetListOfStatefulSets(ns);
        foreach (var s in sts)
        {
            var images = from c in s.Spec.Template.Spec.Containers select c.Image;
            foreach (var image in images)
            {
                liststs.Add(await GenerateContainerUtil(image));
            }
        }
        return liststs ;
    }

    public async ValueTask<Container> GenerateContainerUtil(string image)
    {
        var imageArray = image.Split(new[] {':', '/'}, StringSplitOptions.None);

        return new Container()
        {
            ContainerName = imageArray[^2] == ":" ? imageArray[^3] : imageArray[^1],
            ContainerVersion = imageArray[^1] == ":" ? imageArray[^1] : "latest",
            Image = image,
            Path = image.Substring(image.IndexOf("/", StringComparison.Ordinal)+1)
            //end == -1 ? image.Substring(start) : image.Substring(start, image.Length - end)
        };
    }
    
    public async ValueTask<List<Container>> GetDeployments(string ns)
    {
        var listdeps = new List<Container>();
        var deps = await _kube.GetListOfDeployments(ns);
        foreach (var d in deps)
        {
            var images = from c in d.Spec.Template.Spec.Containers select c.Image;
            foreach (var image in images)
            {
                listdeps.Add(await GenerateContainerUtil(image));
            }
        }
        return listdeps;
    }
    
    public async ValueTask AddOrUpdate(List<ContainerList> containers)
    {
        foreach (var container in containers)
        {
            //This is completely mickey mouse but given we dont need to worry about scaling....
            var documents = await _db.GetDocumentAsync(container.Namespace);
            
            // namespace doesnt exist
            if (documents is null)
            {
                await _db.AddNewList(container);
                await ProcessArtifactoryUpdates(container);
                continue;
            }
            await _db.UpdateContainers(container);
            var process= await ProcessArtifactoryUpdates(container);
            if (process is false)
                throw new ApplicationException("Artifactory Update Failed.");
        }
        
        //namespace exists in db - remove
        var remove = await _db.RemoveMissingDocuments(containers);
        foreach (var r in remove)
        {
            await ProcessArtifactoryUpdates(r);
        }
    }

    public async ValueTask<bool> ProcessArtifactoryUpdates(ContainerList containerList)
    {
        foreach (var c in containerList.Containers)
        {
            var imageArray = c.Image.Split(new[] {':', '/'}, StringSplitOptions.None);
            var result = await _artifactory.UpdateArtifactory(new ArtifactoryCopy()
            {
                copy = true,
                dockerRepository = c.ContainerName,
                targetRepo = imageArray[1],
                targetTag = containerList.Environment
            });
            _logger.LogInformation("Container {name} Updated: {result}", c.ContainerName, result.Success);
            return result.Success;
        }
        return false;
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