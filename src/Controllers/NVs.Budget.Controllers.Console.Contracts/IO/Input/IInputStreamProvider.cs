using FluentResults;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Input;

public interface IInputStreamProvider
{
    Task<Result<StreamReader>> GetInput(string name = "");
}
