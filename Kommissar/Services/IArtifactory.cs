using Kommissar.Model;
using StackExchange.Utils;

namespace Kommissar.Services;

public interface IArtifactory
{
    Task<HttpCallResponse<bool>> UpdateArtifactory(ArtifactoryCopy copy);
}