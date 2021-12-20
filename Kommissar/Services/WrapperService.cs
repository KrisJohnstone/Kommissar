using Kommissar.Repositories;
using Microsoft.Extensions.Logging;

namespace Kommissar.Services;

public class WrapperService : IWrapper
{
    private readonly IKubernetes _kube;
    private readonly ILogger _logger;

    public WrapperService(IKubernetes kube, ILogger<WrapperService> logger)
    {
        _kube = kube;
        _logger = logger;
    }

    public async ValueTask<List<string>> GetEnvList(IEnumerable<string> filter)
    {
        var mbrNamespaces = new List<string>();
        var namespaceList = await _kube.GetListOfEnvs();
        
        foreach (var s in filter)
        {
            mbrNamespaces = new List<string>(from item in namespaceList.Items
                where item.Metadata.Name.Split(new[] {'-'})[0] == s select item.Metadata.Name);
        }
        return mbrNamespaces.ToList();
    }
}