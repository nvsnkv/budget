using FluentResults;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

public interface IInputStreamProvider
{
    Task<Result<StreamReader>> GetInput(string name = "");
}
