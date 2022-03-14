using k8s.Models;

namespace Kommissar.Repositories;
public interface IKubeRepo
{
    ValueTask<V1NamespaceList> GetListOfEnvs();
    ValueTask<List<V1StatefulSet>> GetListOfStatefulSets(string ns);
    ValueTask<List<V1Deployment>> GetListOfDeployments(string ns);
}
    