using System.Text;
using k8s.Models;

namespace Bogus.Kubernetes;

public class KubernetesDataSet : DataSet
{
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
    
    /// <summary>
    /// Returns a container name.
    /// </summary>
    public string ContainerName()
        => Random.ArrayElement(KubernetesStrings.ContainerNames);

    /// <summary>
    /// Returns a container and image version in the format of <image>:<version>
    /// </summary>
    public string Container()
        => $"{Random.ArrayElement(KubernetesStrings.ContainerNames)}:{Version()}";
    
    public string Environment()
        => Random.ArrayElement(KubernetesStrings.Environments);

    /// <summary>
    /// Returns a 3 char representation of a project code.
    /// </summary>
    public string Project()
    {
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
    /// Returns a namespace with a random project and env.
    /// </summary>
    public string NamespaceWithEnvironment()
        => $"{Project()}-{Container()}-{Environment()}";

    public V1Container GenerateContainer()
    {
        return new V1Container()
        {
            Image = Container()
        };
    }
    
}