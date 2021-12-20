namespace Kommissar.Services;

public interface IWrapper
{
    ValueTask<List<string>> GetEnvList(IEnumerable<string> filter);
}