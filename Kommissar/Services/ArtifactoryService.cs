using Kommissar.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Utils;

namespace Kommissar.Services;

public class ArtifactoryService : IArtifactory
{
    private readonly AppSettings _appSettings;
    private readonly ILogger _logger;

    public ArtifactoryService(IOptions<AppSettings> appSettings, ILogger<ArtifactoryService> logger)
    {
        _appSettings = appSettings.Value;
        _logger = logger;
    }
    
    public async Task<HttpCallResponse<bool>> UpdateArtifactory(ArtifactoryCopy copy)
    {
        var result = await Http.Request($"{_appSettings.ArtifactoryBaseUri}/api/docker/{copy.targetRepo}/v2/promote")
            .SendJson(copy)
            .AddHeaders(new Dictionary<string, string>()
                {{"Content-Type","application/json"},
                {"Authorization", $"Basic {_appSettings.ArtifactoryCredential}"}})
            .ExpectHttpSuccess()
            .PostAsync();
        return result;
    }
}