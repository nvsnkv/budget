using FluentResults;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvOperationsReader.Errors;

internal class AccountDoesNotExistError(Guid Id) : IError
{
    public string Message => "Account with such Id does not exist!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(Id), Id } };
    public List<IError> Reasons { get; } = new();
}
