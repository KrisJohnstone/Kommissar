using Bogus.Kubernetes;
using Bogus.Premium;

namespace Bogus;

public static class KubernetesExtension
{
    public static KubernetesDataSet Kubernetes(this Faker faker)
        => ContextHelper.GetOrSet(faker, () => new KubernetesDataSet());
}