using FluentResults;

namespace NVs.Budget.Domain.Errors;

public class OperationDidNotMatchSubcriteriaError : IError
{
    public string Message => "Transaction did not match subcriteria of this logbook";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
