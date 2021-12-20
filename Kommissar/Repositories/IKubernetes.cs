using k8s.Models;
using Microsoft.Rest;

namespace Kommissar.Repositories;
public interface IKubernetes
{
    Task<Task<HttpOperationResponse<V1PodList>>> CreateWatch(IEnumerable<string> names);

    ValueTask<V1NamespaceList> GetListOfEnvs();
}
    