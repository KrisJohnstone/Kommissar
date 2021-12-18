using System;
using System.Text;
using Bogus;
using Bogus.Premium;

namespace Kommissar.Tests;

public static class BogusKubernetesExtension
{
    public static Kubernetes Kubernetes(this Faker faker)
    {
        return ContextHelper.GetOrSet(faker, () => new Kubernetes());
    }
}

public class Kubernetes : DataSet
{
    private static readonly string[] ContainerNames =
    {
        "postgres", "mongodb", "wordpress", "nginx", "elasticsearch", "kind", "flux",
        "sealed-secrets", "argocd", "flagger", "loki", "mariadb", "amplify"
    };

    /// <summary>
    /// Returns a container name.
    /// </summary>
    public string Container()
    => Random.ArrayElement(ContainerNames);

    private static readonly string[] Environments =
    {
        "test", "prod", "development", "acceptance"
    };

    public string Environment()
        => Random.ArrayElement(Environments);

    /// <summary>
    /// Returns a 3 char representation of a project code.
    /// </summary>
    public string Project()
    {
        // creating a StringBuilder object()
        var prefix = new StringBuilder();  
        var random = new Random();  

        char letter;  

        for (var i = 0; i < 3; i++)
        {
            var flt = random.NextDouble();
            var shift = Convert.ToInt32(Math.Floor(25 * flt));
            letter = Convert.ToChar(shift + 65);
            prefix.Append(letter);  
        }
        return prefix.ToString();
    }

    /// <summary>
    /// Returns a namespace with a random env, but without a project.
    /// </summary>
    public string NamespaceWithEnvironment(string containerName)
    => $"{containerName}-{Environment()}";
    
    /// <summary>
    /// Returns a namespace with a random env, but with a project.
    /// </summary>
    public string NamespaceWithEnvironment(string containerName, string project)
        => $"{project}-{containerName}-{Environment()}";
    
    /// <summary>
    /// Returns a namespace with a random env, but with a project.
    /// </summary>
    public string NamespaceWithEnvironment()
        => $"{Project()}-{Container()}-{Environment()}";
    
    /// <summary>
    /// Returns a namespace with a random env, but with a project.
    /// </summary>
    public string NamespaceWithoutEnvironment(string containerName, string project)
        => $"{project}-{containerName}";
    
    /// <summary>
    /// Returns a namespace with a random env, but with a project.
    /// </summary>
    public string NamespaceWithoutEnvironment(string containerName)
        => $"{Project()}-{containerName}";

    public string Version()
    {
        var version = new string("");
        for (var i = 0; i < 3; i++)
        {
            var rd = new Random();
            var randNum = rd.Next(0,20);
            if (i == 0)
            {
                version = $"v{randNum}";
                continue;
            }
            version += $".{randNum}";
        }
        return version;
    }
}