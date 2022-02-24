using System.Text;
using k8s.Models;

namespace Bogus.Kubernetes;

public class KubernetesDataSet : DataSet
{
    public string Version()
    {
        var version = new string("");
        for (var i = 0; i < 3; i++)
        {
            var rd = new Random();
            var randNum = rd.Next(0,20);
            if (i == 0)
            {
                version = $"v{randNum}";
                continue;
            }
            version += $".{randNum}";
        }
        return version;
    }
    
    /// <summary>
    /// Returns a container name.
    /// </summary>
    public string ContainerName()
        => Random.ArrayElement(KubernetesStrings.ContainerNames);

    /// <summary>
    /// Returns a container and image version in the format of <image>:<version>
    /// </summary>
    public string Container()
        => $"{Random.ArrayElement(KubernetesStrings.ContainerNames)}:{Version()}";
    
    public string Environment()
        => Random.ArrayElement(KubernetesStrings.Environments);

    /// <summary>
    /// Returns a 3 char representation of a project code.
    /// </summary>
    public string Project()
        => KubernetesMethods.Project();

    public V1Container GenerateContainer()
    {
        return new V1Container()
        {
            Image = Container()
        };
    }

    public Faker<V1ObjectMeta> GenerateMetadata(string project, string version, string environment, 
                                                                Dictionary<string, string> anno, bool generateLabels)
        => KubernetesMethods.GenerateMetadata(project, version, environment, anno, generateLabels);

    public V1DeploymentList GenerateDeploymentList(string project, string environment, string version,
        int replicas = 3, int availableReplicas = 3, int readyReplicas = 3, int numberOfDeployments = 1)
        => GenerateDeploymentList(project, environment, version, replicas, availableReplicas, replicas, numberOfDeployments);

}