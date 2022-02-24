using System.Collections.Generic;
using System.Text;
using k8s.Models;

namespace Bogus.Kubernetes;

public static class KubernetesMethods
{
    /// <summary>
    /// Returns a 3 char representation of a project code.
    /// </summary>
    public static string Project()
    {
        var prefix = new StringBuilder();
        var random = new Random();

        char letter;

        for (var i = 0; i < 3; i++)
        {
            var flt = random.NextDouble();
            var shift = Convert.ToInt32(Math.Floor(25 * flt));
            letter = Convert.ToChar(shift + 65);
            prefix.Append(letter);
        }
        return prefix.ToString();
    }

    public static Faker<V1ObjectMeta> GenerateMetadata(string project, string version, string environment,
                                                                    Dictionary<string, string> anno, bool generateLabels)
    {
        var faker = new Faker<V1ObjectMeta>()
            .RuleFor(u => u.Name, f => $"{project}-{f.Kubernetes().ContainerName()}")
            .RuleFor(u => u.NamespaceProperty, (f, u) => $"{u.Name}-{environment}");

        if (anno.Count > 0)
        {
            faker.RuleFor(u => u.Annotations, f => anno);
        }

        if (generateLabels)
        {
            faker.RuleFor(u => u.Labels, (f, u) => new Dictionary<string, string>()
            {
                {"app.kubernetes.io/part-of", project},
                {"app.kubernetes.io/managed-by", "fluxcd"},
                {"app.kubernetes.io/created-by", "kustomize-controller"},
                {"app.kubernetes.io/component", u.Name},
                {"app.kubernetes.io/version", version},
                {"app.kubernetes.io/instance", $"{u.Name}-{environment}"},
                {"app.kubernetes.io/name", $"{u.Name}"}
            });
        }
        return faker;
    }

    public static V1DeploymentList GenerateDeploymentList(string project, string environment, string version, 
        int replicas = 3, int availableReplicas = 3, int readyReplicas = 3, int numberOfDeployments = 1)
    {
        var deps = GenerateDeployment(project, environment, version, replicas, availableReplicas, readyReplicas,
            numberOfDeployments);
            
        deps.ForEach(x =>
        {
            x.Spec.Template = new V1PodTemplateSpec()
            {
                Spec = new V1PodSpec()
                {
                    Containers = GenerateContainers(x)
                }
            };
        });
            
        var depList = new Faker<V1DeploymentList>()
            .RuleFor(u => u.Items, (f, u) => deps).Generate();

        return depList;
    }

    public static List<V1Deployment> GenerateDeployment(string project, string environment, string version, int replicas = 3, 
        int availableReplicas = 3, int readyReplicas = 3, int numberOfDeployments = 1)
    {
        var deps = new Faker<V1Deployment>()
            .RuleFor(u => u.Metadata, f => f.Kubernetes()
                .GenerateMetadata(project, environment, version, new Dictionary<string, string>(){{"eyespy-monitor","true"}}, true).Generate())
            .RuleFor(u => u.Spec, (f, u) => new Faker<V1DeploymentSpec>()
                .RuleFor(u => u.Replicas, f => replicas))
            .RuleFor(u => u.Status, f => new V1DeploymentStatus()
            {
                Replicas = replicas,
                AvailableReplicas = availableReplicas,
                ReadyReplicas = readyReplicas
            }).Generate(numberOfDeployments);

        return deps;
    }

    public static List<V1Container> GenerateContainers(V1Deployment deployment, int containerCount = 1)
    => new Faker<V1Container>()
            .RuleFor(u => u.Image, (f,u) => $"{deployment.Name().Split(new []{'-'})[1]}:{deployment.Metadata.Labels["app.kubernetes.io/version"]}")
            .RuleFor(u => u.Name, (f,u) => deployment.Name().Split(new []{'-'})[1]).Generate(containerCount);
    
}