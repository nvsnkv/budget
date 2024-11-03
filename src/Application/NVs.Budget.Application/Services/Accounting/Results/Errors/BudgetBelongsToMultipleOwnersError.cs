using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class BudgetBelongsToMultipleOwnersError : IError
{
    public string Message => "Cannot remove budget with more than one owner!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
