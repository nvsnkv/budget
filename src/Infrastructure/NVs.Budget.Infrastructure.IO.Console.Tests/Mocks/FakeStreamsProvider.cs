using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;

public class FakeStreamsProvider : IOutputStreamProvider, IDisposable, IAsyncDisposable
{
    private readonly MemoryStream _stream = new();

    public Task<StreamWriter> GetOutput(string name = "") => Task.FromResult(new StreamWriter(_stream));

    public byte[] GetOutputBytes() => _stream.ToArray();

    public Task<StreamWriter> GetError(string name = "")
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }

    public void Dispose() => _stream.Dispose();
    public ValueTask DisposeAsync() => _stream.DisposeAsync();
}
