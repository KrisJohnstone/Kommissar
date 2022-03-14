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
using Kommissar.Tests.Mocks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace Kommissar.Tests;

public class KommissarRepoTests
{
    private static DbRepository _kom = new DbRepository(new NullLogger<DbRepository>(), Options.Create(
        new AppSettings()
        {
            DatabaseConnection = "mongodb://root:example@localhost:27017"
        }));

    [Test]
    public async Task ContainerList_Add()
    {
        var x = MockUtils.MockContainerLists("namespace", 1, 3);
        await _kom.AddNewList(x.FirstOrDefault());

        var result = await _kom.GetDocumentAsync("namespace");
        result.Containers.Count().Should().Be(3);
        result.Namespace.Should().Be("namespace");
    }

    [Test]
    public async Task ContainerList_AddContainers()
    {
        var x = MockUtils.MockContainerLists("namespace", 1, 3);
        await _kom.AddNewList(x.FirstOrDefault());

        var conts = MockUtils.MockContainerLists("namespace", 1, 3);
        await _kom.UpdateContainers(conts.FirstOrDefault());

        var result = await _kom.GetDocumentAsync("namespace");
        result.Containers.Count().Should().Be(6);
        result.Namespace.Should().Be("namespace");
    }

    [Test]
    public async Task ContainerList_UpdateExisting()
    {
        var x = MockUtils.MockContainerLists("namespace", 1, 3);
        await _kom.AddNewList(x.FirstOrDefault());

        var conts = x.FirstOrDefault();
        foreach (var container in conts.Containers)
        {
            container.ContainerVersion = "v100.0.5";
        }

        await _kom.UpdateContainers(conts);

        var result = await _kom.GetDocumentAsync("namespace");
        result.Containers.Count().Should().Be(3);
        result.Namespace.Should().Be("namespace");
        result.Containers.FirstOrDefault().ContainerVersion.Should().Be("v100.0.5");
    }

    [Test]
    public async Task ContainerList_RemoveOld()
    {
        var first = MockUtils.MockContainerLists("namespace", 1, 3);
        var second = MockUtils.MockContainerLists("abc", 1, 3);
        await _kom.AddNewList(first.FirstOrDefault());
        await _kom.AddNewList(second.FirstOrDefault());

        await _kom.RemoveMissingDocuments(first);
        var exists = await _kom.GetDocumentAsync("abc");
        var notexist = await _kom.GetDocumentAsync("namespace");
        exists.Should().NotBeNull();
        notexist.Should().BeNull();
    }
}