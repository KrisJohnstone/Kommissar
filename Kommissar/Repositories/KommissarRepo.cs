using System.Collections.Immutable;
using k8s.Models;
using Kommissar.Model;
using Microsoft.Extensions.Logging;

namespace Kommissar.Repositories;

public class KommissarRepo
{
    private readonly ILogger _logger;
    public static Dictionary<string, Container> Data = new();

    public KommissarRepo(ILogger<KommissarRepo> logger)
    {
        _logger = logger;
    }

    public async ValueTask AddOrUpdate(string ns, ImmutableArray<V1Container> containers)
    {
        foreach (var container in containers)
        {
            var split = container.Image.Split(new[] {':'}, StringSplitOptions.TrimEntries);
            var containerName = split[0].Split(new[] { '.' }, StringSplitOptions.None).Last();
            Data.TryGetValue($"{ns}:{containerName}", out var value);

            if (value is null)
            {
                _logger.LogInformation("New Container Added: {containerName} in {ns}", split[0], ns);
                Data.Add($"{ns}:{split[0]}", new Container()
                {
                        ContainerName = containerName,
                        ContainerVersion = split[1]
                });
                continue;
            }

            if (value.ContainerName == split[0] && value.ContainerVersion == split[1])
                continue;

            if (value.ContainerName == split[0] && value.ContainerVersion != split[1])
            {
                _logger.LogInformation("New Container Version Detected: {container}:{containerVersion}", containerName, split[1]);
                Data[$"{ns}:{split[0]}"] = new Container()
                {
                    ContainerName = containerName,
                    ContainerVersion = split[1]
                };
            }
        }
    }

    public async ValueTask<Container> GetContainer(string ns, string container)
    {
        Data.TryGetValue($"{ns}:{container}", out var value);
        return value;
    }
}