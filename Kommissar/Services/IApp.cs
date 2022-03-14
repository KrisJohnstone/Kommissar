using Kommissar.Model;

namespace Kommissar.Services;

public interface IApp
{
    ValueTask Run(string[] args);
    ValueTask<List<string>> GetEnvList(IEnumerable<string> filter);
    ValueTask<List<Container>> GetDeployments(string ns);
    ValueTask<List<Container>> GetStatefulSets(string ns);
}