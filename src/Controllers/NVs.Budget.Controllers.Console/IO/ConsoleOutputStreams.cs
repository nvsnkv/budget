using NVs.Budget.Controllers.Console.Contracts.IO;

namespace NVs.Budget.Controllers.Console.IO;

internal class ConsoleOutputStreams : IOutputStreamProvider
{
    public Task<Stream> GetOutput(string? name = null) => Task.FromResult(System.Console.OpenStandardOutput());

    public Task<Stream> GetError(string? name = null) => Task.FromResult(System.Console.OpenStandardError());
}
