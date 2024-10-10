namespace NVs.Budget.Controllers.Console.Contracts.Commands;

[Flags]
public enum ExitCode
{
    Success = 0,
    OperationError = 3,
    ArgumentsError = 5,
    Cancelled = 7,
    UnexpectedResult = 127
}
