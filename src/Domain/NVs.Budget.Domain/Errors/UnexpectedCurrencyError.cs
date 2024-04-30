using FluentResults;
using NMoneys;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Domain.Errors;

public class UnexpectedCurrencyError(Currency expected, Operation actual) : IError
{
    public string Message => "Unexpected currency given!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(expected), expected }, { nameof(actual), actual.Id }, {nameof(Operation), actual.Id} };
    public List<IError> Reasons { get; } = new();
}
