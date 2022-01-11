using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
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
        var wrapper = new KubeWrapper(mockKubeServiced.Object, Mock.Of<ILogger<KubeWrapper>>(), new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>()));

        for (var i = 0; i < filters.Length; i++)
        {
            var result = await wrapper.GetEnvList(new List<string>() {filters[i]});
            result[i].Should().StartWith(filters[i]);
        }
    }
    
    // [Test]
    // public async Task CreateWatch()
    // {
    //     var kube = new KubernetesService(new NullLogger<KubernetesService>());
    //     var wrapper = new KubeWrapper(kube, new NullLogger<KubeWrapper>(), 
    //         new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>()));
    //     var watch = await kube.CreateWatch(new[] {"default", "kube-system"});
    //     //trigger watch
    //     await wrapper.WatcherCallBack(watch);
    //     //verify its been added to kommmissar repo
    //     
    //     //code here
    //     //deploy change to kube
    //     //code
    //     //verify update has occurred.
    //     //code
    // }

    [Test]
    [TestCase(3, 1, "abc-wordpress")]
    public async Task GetCurrentState_Success(int containersPerPod, int numberOfPods, string ns)
    {
        var mockKube = new MockKubernetes().MockGetListOfPods(ns, 3, 1);
        var kommissar = new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>());
        var wrapper = new KubeWrapper(mockKube.Object, Mock.Of<ILogger<KubeWrapper>>(),
            new KommissarRepo(Mock.Of<ILogger<KommissarRepo>>()));
        
        var x = mockKube.
        await wrapper.GetCurrentState(new List<string>() { "abc-wordpress" });
        var result = await kommissar.GetContainer();
        result.Count.Should().Be(numberOfPods);
        result[numberOfPods-1].Spec.Containers.Count.Should().Be(containersPerPod);
    }
}