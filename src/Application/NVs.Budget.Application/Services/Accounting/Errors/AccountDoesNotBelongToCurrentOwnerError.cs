using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Errors;

internal sealed class AccountDoesNotBelongToCurrentOwnerError : IError
{
    public string Message => "Given account does not belong to current owner!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}