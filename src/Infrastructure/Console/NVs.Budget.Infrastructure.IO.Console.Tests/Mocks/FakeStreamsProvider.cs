using FluentResults;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Infrastructure.IO.Console.Tests.Mocks;

public class FakeStreamsProvider : IInputStreamProvider, IOutputStreamProvider, IDisposable, IAsyncDisposable
{
    private MemoryStream _iStream = new();
    private readonly MemoryStream _oStream = new();

    public Task<Result<StreamReader>> GetInput(string name = "")
    {
        return Task.FromResult(Result.Ok(new StreamReader(_iStream)));
    }

    public void ResetInput(byte[] data)
    {
        _iStream = new MemoryStream(data);
    }

    public Task<StreamWriter> GetOutput(string name = "") => Task.FromResult(new StreamWriter(_oStream));


    public byte[] GetOutputBytes() => _oStream.ToArray();

    public Task<StreamWriter> GetError(string name = "")
    {
        Assert.Fail("This method should not be invoked!");
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _iStream.Dispose();
        _oStream.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await _iStream.DisposeAsync();
        await _oStream.DisposeAsync();
    }

    public async Task ReleaseStreamsAsync()
    {
        await _iStream.FlushAsync();
        await _oStream.FlushAsync();
    }
}
