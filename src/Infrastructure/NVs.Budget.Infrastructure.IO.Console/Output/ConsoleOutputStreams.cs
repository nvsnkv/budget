using System.Collections.Concurrent;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

internal class ConsoleOutputStreams : IOutputStreamProvider, IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, StreamWriter> _outputs = new();
    private readonly ConcurrentDictionary<string, StreamWriter> _errors = new();
    private volatile bool _disposed;

    public ConsoleOutputStreams()
    {
        System.Console.OutputEncoding = System.Text.Encoding.GetEncoding(65001);
    }

    public Task<StreamWriter> GetOutput(string name = "") => Task.FromResult(GetWriter(_outputs, name, CreateOutputWriter));

    public Task<StreamWriter> GetError(string name = "") => Task.FromResult(GetWriter(_errors, name, CreateErrorWriter));

    private StreamWriter GetWriter(ConcurrentDictionary<string, StreamWriter> writers, string name, Func<string,StreamWriter> createWriter)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ConsoleOutputStreams));
        return writers.GetOrAdd(name, createWriter);
    }

    private StreamWriter CreateOutputWriter(string arg)
    {
        var stream = string.IsNullOrEmpty(arg)
            ? System.Console.OpenStandardOutput()
            : File.OpenWrite(arg);

        return new StreamWriter(stream);
    }

    private StreamWriter CreateErrorWriter(string arg)
    {
        var stream = string.IsNullOrEmpty(arg)
            ? System.Console.OpenStandardError()
            : File.OpenWrite(arg);

        return new StreamWriter(stream);
    }

    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        foreach (var (_, writer) in _outputs.Concat(_errors))
        {
            await writer.DisposeAsync();
        }
    }
}
