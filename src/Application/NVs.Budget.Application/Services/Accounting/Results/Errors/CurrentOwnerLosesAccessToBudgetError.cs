using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class CurrentOwnerLosesAccessToBudgetError : IError
{
    public string Message => "Cannot remove current user from the list of owners for budget!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
