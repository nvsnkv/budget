using FluentResults;

namespace NVs.Budget.Domain.Errors;

public class OperationDidNotMatchCriteriaError : IError
{
    public string Message => "Operation did not match criteria of this logbook";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
