using System.Collections.Concurrent;
using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;

namespace NVs.Budget.Controllers.Console.IO.Input;

internal class ConsoleInputStream : IInputStreamProvider, IDisposable
{
    private readonly ConcurrentDictionary<string, StreamReader> _readers = new();
    private volatile bool _disposed;

    public Task<Result<StreamReader>> GetInput(string name = "")
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConsoleInputStream));

        try
        {
            var reader = _readers.GetOrAdd(name, CreateReader);
            return Task.FromResult(Result.Ok(reader));
        }
        catch (Exception e)
        {
            return Task.FromResult(Result.Fail<StreamReader>(new ExceptionBasedError(e)));
        }
    }

    private StreamReader CreateReader(string name)
    {
        var stream = string.IsNullOrEmpty(name)
            ? System.Console.OpenStandardInput()
            : File.OpenRead(name);

        return new StreamReader(stream);
    }

    public void Dispose()
    {
        _disposed = true;

        foreach (var (_, reader) in _readers)
        {
            reader.Dispose();
        }
    }
}
