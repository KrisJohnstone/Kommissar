namespace Bogus.Kubernetes;

public static class KubernetesStrings
{
    public static readonly string[] ContainerNames =
    {
        "postgres", "mongodb", "wordpress", "nginx", "elasticsearch", "kind", "flux",
        "sealed-secrets", "argocd", "flagger", "loki", "mariadb", "amplify"
    };
    
    public static readonly string[] Environments =
    {
        "test", "prod", "development", "acceptance"
    };
}