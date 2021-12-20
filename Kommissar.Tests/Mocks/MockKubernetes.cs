using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using k8s.Models;
using Kommissar.Repositories;
using Kommissar.Services;
using Microsoft.Rest;
using Moq;

namespace Kommissar.Tests.Mocks;

public class MockKubernetes : Mock<IKubernetes>
{
    public MockKubernetes MockGetListOfEnvs(IEnumerable<string> filterList, int namespacesNumber = 4)
    {
        var namespaces = new V1NamespaceList()
        {
            Items = new List<V1Namespace>()
        };
        
        var genericNamespaces = new Faker<V1Namespace>()
                .RuleFor(u => u.Metadata, () => new Faker<V1ObjectMeta>()
                    .RuleFor(u => u.Name, (f, u)
                        => $"{f.Kubernetes().Project()}-{f.Kubernetes().Container()}-{f.Kubernetes().Environment()}"))
                .Generate(50);
        genericNamespaces.ForEach(x => namespaces.Items.Add(x));

        foreach (var filter in filterList)
        {
            var x =  new Faker<V1Namespace>()
                    .RuleFor(u => u.Metadata, () => new Faker<V1ObjectMeta>()
                        .RuleFor(u => u.Name, (f, u)
                            => $"{filter}-{f.Kubernetes().Container()}-{f.Kubernetes().Environment()}"))
                    .Generate(namespacesNumber);
            x.ForEach(i => namespaces.Items.Add(i));
        }

        Setup(m => m.GetListOfEnvs())
            .Returns(ValueTask.FromResult(namespaces));
        return this;
    }

    public MockKubernetes MockCreateWatches(IEnumerable<string> namespaces)
    {
        var pods = new Faker<V1Pod>()
            .RuleFor(u => u.Spec, (f, _) => new V1PodSpec()
            {
                Containers = new List<V1Container>()
                {
                    new ()
                    {
                        Name = f.Kubernetes().Container(),
                        Image = $"{Name}:{f.Kubernetes().Version()}",
                    }
                }
            })
            .RuleFor(u => u.Metadata.Namespace(), (f, u) => f.Kubernetes().NamespaceWithEnvironment(u.Spec.Containers.FirstOrDefault().Name));


        Setup(m => m.CreateWatch(namespaces))
            .Returns(Task.FromResult(new Task<HttpOperationResponse<V1PodList>>(() => new HttpOperationResponse<V1PodList>()
            {
                Body = new V1PodList()
                {
                    Items = pods.Generate(5),
                }
                
            })));
        return this;
    }
}