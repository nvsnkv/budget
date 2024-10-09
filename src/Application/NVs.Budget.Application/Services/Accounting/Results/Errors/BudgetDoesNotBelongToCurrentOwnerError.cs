using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class BudgetDoesNotBelongToCurrentOwnerError : IError
{
    public string Message => "Given budget does not belong to current owner!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
