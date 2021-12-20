using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Kommissar.Services;
using Kommissar.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Kommissar.Tests;

public class KubernetesTest
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase(1)]
    [TestCase(5)]
    public async Task GetNamespaces(int numberOfNameSpaces)
    {
        // var mockKubeServiced = new MockKubernetes()
        //     .MockGetListOfEnvs( new List<string>(){"mbr"}, numberOfNameSpaces);
        //
        // var result = await mockKubeServiced.Object
        //     .GetListofEnvs(new List<string>{"mbr"});
        //
        // result[0].Should().StartWith("mbr");
        // result.Should().HaveCount(numberOfNameSpaces);

        var mockKubeServiced = new MockKubernetes()
            .MockGetListOfEnvs( new List<string>(){"mbr"}, numberOfNameSpaces);
        var wrapper = new WrapperService(mockKubeServiced.Object, Mock.Of<ILogger<WrapperService>>());

        var result = await wrapper.GetEnvList(new List<string>() {"mbr"});
        
        result[0].Should().StartWith("mbr");
        result.Should().HaveCount(numberOfNameSpaces);
    }

    [Test]
    public async Task CreateWatch()
    {
        
    }
}