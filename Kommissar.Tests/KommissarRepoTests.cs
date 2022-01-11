using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using k8s.Models;
using Kommissar.Model;
using Kommissar.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Kommissar.Tests;

public class KommissarRepoTests
{
    private static KommissarRepo _kom = new KommissarRepo(new NullLogger<KommissarRepo>());
    
    private static async Task<List<V1Container>> AddContainers(string ns, int numberOfInstances)
    {
        var x = new Faker<V1Container>()
            .RuleFor(u => u.Image, (f, u) => f.Kubernetes().Container())
            .Generate(numberOfInstances);
        
        await _kom.AddOrUpdate("abc-wordpress", ImmutableArray.Create<V1Container>(x.ToArray()));
        return x;
    }

    [Test]
    [TestCase(1)]
    [TestCase(3)]
    public async Task Kommissar_AddTest(int number)
    {
        var containers = await AddContainers("abc-wordpress", number);
        
        //Check the container was added.
        foreach (var container in containers)
        {
            var result = await _kom.GetContainer("abc-wordpress", container.Image.Split(new[] {':'})[0]);
            var containerSplit = container.Image.Split(new[] {':'});
            result.ContainerName.Should().Be(containerSplit[0]);
            result.ContainerVersion.Should().Be(containerSplit[1]);
        }
        containers.Count.Should().Be(number);
    }

    [Test]
    public async Task Kommissar_UpdateTest()
    {
        var containers = await AddContainers("abc-wordpress", 1);

        var originalContainer = await _kom.GetContainer("abc-wordpress", containers
            .FirstOrDefault()?.Image.Split(new[] {':'}, StringSplitOptions.TrimEntries)[0]);
        
        //Copy
        var v = originalContainer.ContainerVersion.Split(new[] {'v', '.'}, StringSplitOptions.TrimEntries);

        await _kom.AddOrUpdate("abc-wordpress", ImmutableArray.Create(new V1Container()
        {
            Image = $"{originalContainer.ContainerName}:{v[0]}.{v[1]}.{v[2]+1}"
        }));
        var result = await _kom.GetContainer("abc-wordpress", originalContainer.ContainerName);

        result.ContainerName.Should().Be(originalContainer.ContainerName);
        result.ContainerVersion.Should().NotBeSameAs(originalContainer.ContainerVersion);
    }
}