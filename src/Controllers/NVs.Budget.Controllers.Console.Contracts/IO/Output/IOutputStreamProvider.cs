namespace NVs.Budget.Controllers.Console.Contracts.IO.Output;

public interface IOutputStreamProvider
{
    Task<Stream> GetOutput(string? name = null);
    Task<Stream> GetError(string? name = null);
}
