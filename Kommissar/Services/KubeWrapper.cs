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
    
    public async ValueTask WatcherCallBack(List<string> namespaces)
    {
        var timer = Stopwatch.StartNew();
        timer.Start();

        var pods = await _kube.CreateWatch(namespaces);
        var watches = new List<Task>();
        foreach (var task in pods)
        {
            watches.Add(Watcher(task));
        }
        await Task.WhenAll(watches);
    }

    private async Task Watcher(Task<HttpOperationResponse<V1PodList>> task)
    {
        var timer = Stopwatch.StartNew();
        timer.Start();
        
        await foreach (var (type, item) in task.WatchAsync<V1Pod, V1PodList>())
        {
            _logger.LogInformation("Event Received of type: " +
                                   "{type} in {namespace}", type, item.Metadata.NamespaceProperty);
            
            if(type == WatchEventType.Added)
            {
               //trigger update to repos
               await _kommissar.AddOrUpdate(item.Namespace(), item.Spec.Containers.ToImmutableArray());
            }

            if (type == WatchEventType.Error)
            {
                //trigger what??
            }

            if (type == WatchEventType.Deleted)
            {
                //trigger update to repos
            }
            
            if (type == WatchEventType.Modified)
            {
                //trigger update to repos
            }
        }
        _logger.LogInformation("Watch Event Lasted {time}", timer.ElapsedMilliseconds);
    }

    public async ValueTask GetCurrentState(List<string> namespaces)
    {
        foreach (var ns in namespaces)
        {
            var pods = await _kube.GetListOfPods(ns);
            foreach (var pod in pods.Items)
            {
                await _kommissar.AddOrUpdate(pod.Namespace(), pod.Spec.Containers.ToImmutableArray());
            }
        }
    }
}