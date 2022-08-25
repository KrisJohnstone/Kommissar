using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Kommissar.Model;
using Kommissar.Repositories;
using Kommissar.Services;
using Kommissar.Tests.Mocks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace Kommissar.Tests;

public class AppServiceTests
{
    //Should create a base class that tests extends to stop methods like this being copied across, buuuuuut for now cbf.
    //Should moq this so that theres no dependency on standing mongo up. Buuuuuuut cbf doing that either.
    private static DbRepository _db = new DbRepository(new NullLogger<DbRepository>(), Options.Create(
        new AppSettings()
        {
            DatabaseConnection = "mongodb://root:example@localhost:27017"
        }));
    
    [Test]
    public async Task GetDeployments()
    {
        var mock = MockUtils.MockGetListOfDeployments(1);
        var mockKube = new KubernetesServiceMocks().MockGetListOfDeployments(mock, "namespace");
        var service = new AppService(Options.Create(new AppSettings()), new NullLogger<AppService>(), 
            mockKube.Object,Mock.Of<IDbRepository>(), Mock.Of<IArtifactory>());

        var result = await service.GetDeployments("namespace");
        result.FirstOrDefault()
            ?.Image.Should()
            .Be(mock.FirstOrDefault()?.Spec.Template.Spec.Containers.FirstOrDefault()?.Image);
    }

    [Test]
    public async Task GetStatefulSets()
    {
        var mock = MockUtils.MockGetListOfStatefulsets(1);
        var mockKube = new KubernetesServiceMocks().MockGetListOfStatefulsets(mock, "namespace");
        var service = new AppService(Options.Create(new AppSettings()), new NullLogger<AppService>(), 
            mockKube.Object,Mock.Of<IDbRepository>(), Mock.Of<IArtifactory>());

        var result = await service.GetStatefulSets("namespace");
        result.FirstOrDefault()
            ?.Image.Should()
            .Be(mock.FirstOrDefault()?.Spec.Template.Spec.Containers.FirstOrDefault()?.Image);
    }

    [Test]
    public async Task AddOrUpdate_Add()
    {
        var service = new AppService(Options.Create(new AppSettings()), new NullLogger<AppService>(), 
            Mock.Of<IKubeRepo>(), _db, Mock.Of<IArtifactory>());

        var mock = MockUtils.MockContainerLists("namespace");
        await service.AddOrUpdate(mock);
        var result = await _db.GetDocumentAsync("namespace");
        
        result.Containers.FirstOrDefault().Should()
            .Be(mock.FirstOrDefault()?.Containers.FirstOrDefault()?.ContainerName);
    }
    
    [Test]
    public async Task AddOrUpdate_Update()
    {
        var mock = MockUtils.MockContainerLists("namespace");
        mock.AddRange(MockUtils.MockContainerLists("abc"));
        
        var service = new AppService(Options.Create(new AppSettings()), new NullLogger<AppService>(), 
            Mock.Of<IKubeRepo>(), _db, Mock.Of<IArtifactory>());

        await service.AddOrUpdate(mock);


        mock.ForEach(x =>
            x.Containers.FirstOrDefault().ContainerVersion =
                x.Containers.FirstOrDefault()?.ContainerVersion.Split(new[] {'.'})[2] + 1);

        await service.AddOrUpdate(mock);

        var result = await _db.GetDocumentAsync("namespace");
        result.Containers.FirstOrDefault().Should()
            .Be(mock.FirstOrDefault()?.Containers.FirstOrDefault()?.ContainerName);
    }
    
}