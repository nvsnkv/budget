using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Controllers.Console.IO.Output;

internal class ConsoleOutputStreams : IOutputStreamProvider
{
    public Task<Stream> GetOutput(string? name = null) => Task.FromResult(System.Console.OpenStandardOutput());

    public Task<Stream> GetError(string? name = null) => Task.FromResult(System.Console.OpenStandardError());
}
