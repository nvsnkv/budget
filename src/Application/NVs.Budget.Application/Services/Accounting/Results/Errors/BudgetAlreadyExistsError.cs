using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class BudgetAlreadyExistsError : IError
{
    public string Message => "Budget with these name and bank name already exists!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
