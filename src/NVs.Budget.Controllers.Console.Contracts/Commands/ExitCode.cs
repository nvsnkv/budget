namespace NVs.Budget.Controllers.Console.Contracts.Commands;

public enum ExitCode
{
    Success = 0,
    OperationError = 3,
    ArgumentsError = 5,
    UnexpectedResult = 127
}