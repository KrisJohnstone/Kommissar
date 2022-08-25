using Kommissar.Model;

namespace Kommissar.Repositories;

public interface IDbRepository
{
    ValueTask<ContainerList> GetDocumentAsync(string ns);
    ValueTask AddNewList(ContainerList container);
    ValueTask UpdateContainers(ContainerList containerList);
    ValueTask<List<ContainerList>> RemoveMissingDocuments(List<ContainerList> list);
}