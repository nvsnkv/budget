using FluentResults;

namespace NVs.Budget.Domain.Errors;

public class TransactionDidNotMatchSubcriteriaError : IError
{
    public string Message => "Transaction did not match subcriteria of this logbook";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}