using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using FluentAssertions;
using Kommissar.Tests.Mocks;
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
        var mockKubeServiced = new MockKubernetes()
            .MockGetListofEnvs( new List<string>(){"mbr"}, numberOfNameSpaces);

        var result = await mockKubeServiced.Object
            .GetListofEnvs(new List<string>{"mbr"});

        result.Should().HaveCount(numberOfNameSpaces);
    }

    [Test]
    public async Task CreateWatch()
    {
        
    }
}