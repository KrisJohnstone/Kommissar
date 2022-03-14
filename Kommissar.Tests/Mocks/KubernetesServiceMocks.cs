using System.Collections.Generic;
using System.Threading.Tasks;
using k8s.Models;
using Kommissar.Repositories;
using Moq;

namespace Kommissar.Tests.Mocks;

public class KubernetesServiceMocks : Mock<IKubeRepo>
{
    public KubernetesServiceMocks MockGetListOfStatefulsets(List<V1StatefulSet> sts, string ns)
    {
        Setup(x => x.GetListOfStatefulSets(ns)).Returns(ValueTask.FromResult(sts));
        return this;
    }
    
    public KubernetesServiceMocks MockGetListOfDeployments(List<V1Deployment> deps, string ns)
    {
        Setup(x => x.GetListOfDeployments(ns)).Returns(ValueTask.FromResult(deps));
        return this;
    }
}