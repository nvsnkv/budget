using FluentResults;

namespace NVs.Budget.Domain.Errors;

public class TransactionDidNotMatchCriteriaError : IError
{
    public string Message => "Transaction did not match criteria of this logbook";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
