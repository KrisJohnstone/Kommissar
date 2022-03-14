using System.Collections.Generic;
using Bogus;
using Bogus.Kubernetes;
using k8s.Models;
using Kommissar.Model;

namespace Kommissar.Tests.Mocks;

public static class MockUtils
{
    public static V1NamespaceList MockGetListOfEnvs(IEnumerable<string> filterList,
        int namespacesNumber = 4)
    {
        var namespaces = new V1NamespaceList()
        {
            Items = new List<V1Namespace>()
        };

        var genericNamespaces = new Faker<V1Namespace>()
            .RuleFor(u => u.Metadata, () => new Faker<V1ObjectMeta>()
                .RuleFor(u => u.Name, (f, u)
                    => $"{f.Kubernetes().Project()}-{f.Kubernetes().ContainerName()}-{f.Kubernetes().Environment()}"))
            .Generate(50);
        genericNamespaces.ForEach(x => namespaces.Items.Add(x));

        foreach (var filter in filterList)
        {
            var x = new Faker<V1Namespace>()
                .RuleFor(u => u.Metadata, () => new Faker<V1ObjectMeta>()
                    .RuleFor(u => u.Name, (f, u)
                        => $"{filter}-{f.Kubernetes().ContainerName()}-{f.Kubernetes().Environment()}"))
                .Generate(namespacesNumber);
            x.ForEach(i => namespaces.Items.Add(i));
        }

        return namespaces;
    }
    
    public static List<V1StatefulSet> MockGetListOfStatefulsets(int numberOfPods)
    {
        var stsList = new List<V1StatefulSet>();

        stsList.AddRange(KubernetesMethods.GenerateStatefulset(numberOfPods));

        return stsList;
    }
    
    public static List<V1Deployment> MockGetListOfDeployments(int numberOfPods)
    {
        var depList = new List<V1Deployment>();

        depList.AddRange(KubernetesMethods.GenerateDeployment(numberOfPods));

        return depList;
    }
    
    public static List<ContainerList> MockContainerLists(string ns, int numberOfContainerLists = 1, int numberOfContainers = 1)
    {
        return new Faker<ContainerList>()
            .RuleFor(i => i.Namespace, (faker, _) => ns)
            .RuleFor(i => i.Containers, () => new Faker<Container>()
                .RuleFor(i => i.ContainerName, (faker, _) =>
                    faker.Kubernetes().Container())
                .RuleFor(i => i.ContainerVersion, (faker, _) => faker.Kubernetes().Version())
                .RuleFor(i => i.FullPath, (_, container) => $"{container.ContainerName}:{container.ContainerVersion}")
                .Generate(numberOfContainers)).Generate(numberOfContainerLists);
    }
}