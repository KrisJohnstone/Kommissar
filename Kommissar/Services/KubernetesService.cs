using System.Collections.Immutable;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Kommissar.Services;
public class KubernetesService : IKubernetes
{
    private readonly ILogger _logger;

    public KubernetesService(ILogger<KubernetesService> logger)
    {
        _logger = logger;
    }

    private async ValueTask<Kubernetes> GetClient()
    {
        _logger.LogInformation("Loading kubeconf");
        var config = KubernetesClientConfiguration.IsInCluster()
            ? KubernetesClientConfiguration.InClusterConfig()
            : KubernetesClientConfiguration.BuildDefaultConfig();
        _logger.LogDebug("KubeConfig Loaded:{config}", config);
        return new Kubernetes(config);
    }

    public async ValueTask<List<string>> GetListofEnvs(IEnumerable<string> filter)
    {
        _logger.LogInformation("Retrieving List of Namespaces");
        var kube = await GetClient();
        var nameSpaceList = await kube.ListNamespaceWithHttpMessagesAsync();
        var mbrNamespaces = new List<string>();
        
        foreach (var s in filter)
        {
            mbrNamespaces = new List<string>(from item in nameSpaceList.Body.Items
                where item.Metadata.Name.Split(new[] {'-'})[0] == s select item.Metadata.Name);
        }
        return mbrNamespaces.ToList();
    }
    
    public async Task<Task<HttpOperationResponse<V1PodList>>> CreateWatch(IEnumerable<string> names)
    {
        _logger.LogInformation("Creating Watchers");
        var client = await GetClient();
        Task<HttpOperationResponse<V1PodList>> podlistResp = null;

        foreach (var name in names)
        {
            _logger.LogInformation("Creating Watcher for {name}", name);
            podlistResp = client.ListNamespacedPodWithHttpMessagesAsync(name, watch: true);
        }
        return podlistResp;
    }
}