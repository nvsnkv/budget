using FluentResults;

namespace NVs.Budget.Controllers.Console.Contracts.IO;

public interface IOutputStreamProvider
{
    Task<Stream> GetOutput(string? name = null);
    Task<Stream> GetError(string? name = null);
}
