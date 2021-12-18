using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using k8s.Models;
using Microsoft.Rest;

namespace Kommissar.Services;
public interface IKubernetes
{
    Task<Task<HttpOperationResponse<V1PodList>>> CreateWatch(IEnumerable<string> names);

    ValueTask<List<string>> GetListofEnvs(IEnumerable<string> filter);
}
    