using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using k8s.Models;
using Kommissar.Services;
using Microsoft.Rest;
using Moq;

namespace Kommissar.Tests.Mocks;

public class MockKubernetes : Mock<IKubernetes>
{
    public MockKubernetes MockGetListofEnvs(IEnumerable<string> filter, int namespacesPerFilter = 1)
    {
        var namepsaces = new List<string>();
        foreach (var prefix in filter)
        {
            // var x = new Faker<V1Pod>()
            //     .RuleFor(u => u.Metadata.Namespace(), (f, u) => f.Kubernetes().NamespaceWithEnvironment(prefix));
            var x = new Faker<V1Pod>()
                .RuleFor(u => u.Metadata, f => new Faker<V1ObjectMeta>()
                    .RuleFor(u => u.NamespaceProperty, (f, u)
                        => $"{f.Kubernetes().Container()}-{f.Kubernetes().Environment()}"));

            var generated = x.Generate(namespacesPerFilter);
            generated.ForEach(x => namepsaces.Add( x.Metadata.Namespace()));
        }
        
        Setup(m => m.GetListofEnvs(filter))
            .Returns(ValueTask.FromResult(namepsaces));
        return this;
    }

    public MockKubernetes MockCreateWatches(IEnumerable<string> namespaces)
    {
        var pods = new Faker<V1Pod>()
            .RuleFor(u => u.Spec, (f, u) => new V1PodSpec()
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