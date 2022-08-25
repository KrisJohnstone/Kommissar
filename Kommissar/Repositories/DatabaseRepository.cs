using Kommissar.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace Kommissar.Repositories;

public class DbRepository : IDbRepository
{
    private readonly ILogger _logger;
    private readonly AppSettings _appSettings;
    private readonly IMongoCollection<ContainerList> _mongo;

    public DbRepository(ILogger<DbRepository> logger, IOptions<AppSettings> appSettings)
    {
        _logger = logger;
        _appSettings = appSettings.Value;
        _mongo = new MongoClient(_appSettings.DatabaseConnection)
            .GetDatabase("kommissar")
            .GetCollection<ContainerList>("kommissar");
    }

    public async ValueTask<ContainerList> GetDocumentAsync(string ns)
    {
        return await _mongo.Find(x => x.Namespace == ns).FirstOrDefaultAsync();
    }

    public async ValueTask AddNewList(ContainerList container)
    {
        _logger.LogInformation("New Namespace Added: {nameSpace}", container.Namespace);
        await _mongo.InsertOneAsync(container);
    }

    public async ValueTask UpdateContainers(ContainerList containerList)
    {
        _logger.LogInformation("New Containers Added For: {ns}", containerList.Namespace);
        await _mongo.ReplaceOneAsync(x => x.Id == containerList.Id, containerList);
    }

    public async ValueTask<List<ContainerList>> RemoveMissingDocuments(List<ContainerList> list)
    {
        var remove = new List<string>();
        
        foreach (var container in list)
        {
            remove.Add(container.Namespace);
        }

        var filter = Builders<ContainerList>.Filter.Nin(d => d.Namespace, remove);
        var documents = await _mongo.FindAsync(filter);

        await _mongo.DeleteManyAsync(filter);
        return documents.ToList();
    }
}