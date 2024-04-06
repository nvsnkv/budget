using FluentResults;

namespace NVs.Budget.Controllers.Console.Contracts.Commands;

public static class ExitCodeHelpers
{
    public static ExitCode ToExitCode(this IResultBase result)
    {
        return result.IsSuccess ? ExitCode.Success : ExitCode.OperationError;
    }
}
