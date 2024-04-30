using FluentResults;
using NMoneys;

namespace NVs.Budget.Domain.Errors;

public class UnexpectedCurrencyError(Currency expected, Currency actual) : IError
{
    public string Message => "Unexpected currency given!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(expected), expected }, { nameof(actual), actual } };
    public List<IError> Reasons { get; } = new();
}
