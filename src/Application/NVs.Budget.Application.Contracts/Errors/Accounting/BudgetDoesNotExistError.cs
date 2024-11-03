using FluentResults;

namespace NVs.Budget.Application.Contracts.Errors.Accounting;

public class BudgetDoesNotExistError(Guid Id) : IError
{
    public string Message => "Budget with such Id does not exist!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(Id), Id } };
    public List<IError> Reasons { get; } = new();
}
