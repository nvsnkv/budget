using FluentResults;

namespace NVs.Budget.Controllers.Console.Commands;

internal enum ExitCodes
{
    Success = 0,
    OperationError = 3,
    ArgumentsError = 5,
    UnexpectedResult = 127
}

internal static class ExitCodeHelpers
{
    public static int ToExitCode(this IResultBase result)
    {
        return result.IsSuccess ? (int)ExitCodes.Success : (int)ExitCodes.OperationError;
    }
}
