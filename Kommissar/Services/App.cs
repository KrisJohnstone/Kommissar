using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Kommissar.Model;
using Kommissar.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Nito.AsyncEx;

namespace Kommissar.Services;
public class App
{
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;
    private readonly IKubeWrapper _wrapper;
    private readonly IKubeWrapper _kube;

    public App(IOptions<AppSettings> appSettings, ILogger<App> logger, IKubeWrapper wrapper, IKubeWrapper kube)
    {
        _logger = logger;
        _wrapper = wrapper;
        _kube = kube;
        _appSettings = appSettings.Value;
    }

    public async ValueTask Run(string[] args)
    {
        var namespaces = await _wrapper.GetEnvList(
                                                    _appSettings.Namespaces.ToImmutableArray());
        if (namespaces.Count == 0)
        {
            _logger.LogCritical("No Namespaces Returned from Cluster");
            return;
        }
        // First we need to populate the dataset with current state.
        
        
        
        
        
        //var watchlist = await _kube.CreateWatch(namespaces);
        //
        // if (watchlist.Result.Body is not null || watchlist.Result.Body.Items.Count > 0)
        // {
        //     await WatcherCallBack(watchlist);
        // }
    }

    public async ValueTask PopulateCache(List<string> namespaces)
    {
        
    }
}