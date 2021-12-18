using System.Collections.Immutable;
using System.Diagnostics;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Kommissar.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Rest;
using Nito.AsyncEx;

namespace Kommissar.Services;
public class App
{
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;
    private readonly IKubernetes _kube;
    private readonly KommissarRepo _kommissar;

    public App(IOptions<AppSettings> appSettings, ILogger<App> logger, IKubernetes kube, KommissarRepo kommissar)
    {
        _logger = logger;
        _kube = kube;
        _kommissar = kommissar;
        _appSettings = appSettings.Value;
    }

    public async ValueTask Run(string[] args)
    {
        var namespaces = await _kube.GetListofEnvs(
                                                    _appSettings.Namespaces.ToImmutableArray());
        if (namespaces.Count == 0)
        {
            _logger.LogCritical("No Namespaces Returned from Cluster");
            return;
        }
        var watchlist = await _kube.CreateWatch(namespaces);

        if (watchlist.Result.Body is not null || watchlist.Result.Body.Items.Count > 0)
        {
            await WatcherCallBack(watchlist);
        }
    }

    public async ValueTask WatcherCallBack(Task<HttpOperationResponse<V1PodList>> podlist)
    {
        var timer = Stopwatch.StartNew();
        timer.Start();
        using (podlist.Watch<V1Pod, V1PodList>(async (type, item) =>
           {
               _logger.LogInformation("Event Received of type: " +
                                      "{type} in {namespace}", type, item.Metadata.NamespaceProperty);
               
               await _kommissar.AddOrUpdate(item.Namespace(), item.Spec.Containers.ToImmutableArray());
           },
           error =>
           {
               _logger.LogError(error, "Error Received while Watching");
           },
           () =>
           {
               _logger.LogInformation("Server Disconnected at {time}", timer.ElapsedMilliseconds);
               timer.Stop();
               new AsyncManualResetEvent().Set();
           })) { }
    }
}