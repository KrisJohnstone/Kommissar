using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using k8s.Models;
using Kommissar.Repositories;
using Kommissar.Services;
using Kommissar.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Kommissar.Tests;

public class KubeWrapperTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase(new object[] {"abc", "def"})]
    [TestCase(new object[]{"abc"})]
    public async Task NamespaceFilterTest_Filters(params string[] filters)
    {
        var mockKubeServiced = new MockKubernetes()
            .MockGetListOfEnvs( filters);
        var wrapper = new KubeWrapper(mockKubeServiced.mockKubernetes.Object, Mock.Of<ILogger<KubeWrapper>>(), new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>()));

        for (var i = 0; i < filters.Length; i++)
        {
            var result = await wrapper.GetEnvList(new List<string>() {filters[i]});
            result[i].Should().StartWith(filters[i]);
        }
    }
    
    [Test]
    [TestCase(new object?[] {"default", "kube-system"})]
    public async Task CreateWatch(string[] ns)
    {
        //Setup Data
        var tuple = new MockKubernetes().MockGetListOfPods(ns[0], 1, 1);
        var kommissar = new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>());
        var wrapper = new KubeWrapper(tuple.mockKubernetes.Object, Mock.Of<ILogger<KubeWrapper>>(),
            kommissar);
        await wrapper.GetCurrentState(new List<string>() {ns[0]});
        
        var kube = new KubernetesService(Mock.Of<ILogger<KubernetesService>>());
       
        var watch = await kube.CreateWatch(ns.ToList());
        //trigger watch
        await wrapper.WatcherCallBack(watch);
      
        //code here
        
        //deploy change to kube
        //code
        //verify update has occurred.
        //code
    }

    
    [Test]
    [TestCase(3, 1, "abc-one")]
    [TestCase(1, 2, "abc-two")]
    public async Task GetCurrentState_Success(int containersPerPod, int numberOfPods, string ns)
    {
        var tuple = new MockKubernetes().MockGetListOfPods(ns, containersPerPod, numberOfPods);
        var kommissar = new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>());
        var wrapper = new KubeWrapper(tuple.mockKubernetes.Object, Mock.Of<ILogger<KubeWrapper>>(),
            kommissar);
        
        await wrapper.GetCurrentState(new List<string>() {ns});
        
        foreach (var pod in tuple.podList)
        {
            foreach (var container in pod.Spec.Containers)
            {
                var testData = container.Image.Split(new[] { ':' });
                var result = await kommissar.GetContainer(pod.Metadata.NamespaceProperty, testData[0]);
                
                result.ContainerName.Should().Be(testData[0]);
                result.ContainerVersion.Should().Be(testData[1]);
            }
        }
        tuple.podList.Count.Should().Be(numberOfPods);
        tuple.podList[numberOfPods-1].Spec.Containers.Count.Should().Be(containersPerPod);
    }
}