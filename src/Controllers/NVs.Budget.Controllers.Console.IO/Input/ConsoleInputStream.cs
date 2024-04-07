using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;

namespace NVs.Budget.Controllers.Console.IO.Input;

internal class ConsoleInputStream : IInputStreamProvider
{
    public Task<Result<Stream>> GetInput(string? name = null)
    {
        if (name is null)
        {
            return Task.FromResult(Result.Ok(System.Console.OpenStandardInput()));
        }

        try
        {
            return Task.FromResult(Result.Ok((Stream)File.OpenRead(name)));
        }
        catch (Exception e)
        {
            return Task.FromResult(Result.Fail<Stream>(new ExceptionBasedError(e)));
        }
    }
}
