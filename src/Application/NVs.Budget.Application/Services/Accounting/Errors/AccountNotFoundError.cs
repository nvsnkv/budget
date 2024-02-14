using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Errors;

internal class AccountNotFoundError : IError
{
    public string Message => "Account was not found!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}