using System.Collections.Generic;
using Bogus;
using Bogus.Kubernetes;
using k8s.Models;
using Kommissar.Model;

namespace Kommissar.Tests.Mocks;

public static class MockUtils
{
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
                .RuleFor(i => i.Image, (_, container) => $"{container.ContainerName}:{container.ContainerVersion}")
                .RuleFor(i => i.Path, (_, container) => $"repo/{container.ContainerName}:{container.ContainerVersion}")
                .Generate(numberOfContainers)).Generate(numberOfContainerLists);
    }
}