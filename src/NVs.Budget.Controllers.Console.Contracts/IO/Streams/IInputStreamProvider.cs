using FluentResults;

namespace NVs.Budget.Controllers.Console.Contracts.IO;

public interface IInputStreamProvider
{
    Task<Result<Stream>> GetInput(string? name = null);
}
