using System.Collections.Immutable;
using System.Diagnostics;
using AutoMapper;
using k8s.Models;
using k8s;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Nito.AsyncEx;
using Kommissar.Repositories;

namespace Kommissar.Services;

public class KubeWrapper : IKubeWrapper
{
    private readonly IKubeRepo _kube;
    private readonly ILogger _logger;
    private readonly KommissarRepo _kommissar;
    
    public KubeWrapper(IKubeRepo kube, ILogger<KubeWrapper> logger, KommissarRepo kommissar)
    {
        _kube = kube;
        _logger = logger;
        _kommissar = kommissar;
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
    
    public async ValueTask WatcherCallBack(HttpOperationResponse<V1PodList> podlist)
    {
        var timer = Stopwatch.StartNew();
        timer.Start();
        using (podlist.Watch<V1Pod, V1PodList>(async (type, item) =>
           {
               _logger.LogInformation("Event Received of type: " +
                                      "{type} in {namespace}", type, item.Metadata.NamespaceProperty);
       
               await _kommissar.AddOrUpdate(item.Namespace(), item.Spec.Containers.ToImmutableArray());
           },
           error =>
           {
               _logger.LogError(error, "Error Received while Watching");
           },
           () =>
           {
               _logger.LogInformation("Server Disconnected at {time}", timer.ElapsedMilliseconds);
               timer.Stop();
               new AsyncManualResetEvent().Set();
           })) { }
    }

    public async ValueTask GetCurrentState(List<string> namespaces)
    {
        foreach (var ns in namespaces)
        {
            var pods = await _kube.GetListOfPods(ns);
            foreach (var podsItem in pods.Items)
            {
                await _kommissar.AddOrUpdate(podsItem.Namespace(), podsItem.Spec.Containers.ToImmutableArray());
            }
        }
    }
}