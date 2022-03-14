using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;

namespace Kommissar.Repositories;
public class KubernetesService : IKubeRepo
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

    public async ValueTask<List<V1Deployment>> GetListOfDeployments(string ns)
    {
        _logger.LogInformation("Retrieving List of Deployments");
        var kube = await GetClient();
        var deps = await kube.ListNamespacedDeploymentWithHttpMessagesAsync(ns);
        return deps.Body.Items as List<V1Deployment>;
    }
    
    public async ValueTask<List<V1StatefulSet>> GetListOfStatefulSets(string ns)
    {
        _logger.LogInformation("Retrieving List of Deployments");
        var kube = await GetClient();
        var deps = await kube.ListNamespacedStatefulSetWithHttpMessagesAsync(ns);
        return deps.Body.Items as List<V1StatefulSet>;
    }

    public async ValueTask<V1PodList> GetListOfPods(string ns)
    {
        _logger.LogInformation("Retrieving pods in {ns}", ns);
        var client = await GetClient();
        var pods = await client.ListNamespacedPodWithHttpMessagesAsync(ns);
        return pods.Body;
    }
}