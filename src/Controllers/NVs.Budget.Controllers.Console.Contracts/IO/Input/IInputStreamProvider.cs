using FluentResults;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Input;

public interface IInputStreamProvider
{
    Task<Result<Stream>> GetInput(string? name = null);
}
