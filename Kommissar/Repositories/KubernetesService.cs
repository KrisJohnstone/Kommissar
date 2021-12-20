using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;

namespace Kommissar.Repositories;
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

    public async ValueTask<V1NamespaceList> GetListOfEnvs()
    {
        _logger.LogInformation("Retrieving List of Namespaces");
        var kube = await GetClient();
        var nameSpaceList = await kube.ListNamespaceWithHttpMessagesAsync();
        return nameSpaceList.Body;
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