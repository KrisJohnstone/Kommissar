using k8s.Models;

namespace Kommissar.Services;

public interface IKubeWrapper
{
    ValueTask<List<string>> GetEnvList(IEnumerable<string> filter);

    ValueTask<List<V1Pod>> GetPodList(List<string> namespaces);
}