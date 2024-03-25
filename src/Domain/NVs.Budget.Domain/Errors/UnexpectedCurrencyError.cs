using FluentResults;

namespace NVs.Budget.Domain.Errors;

public class UnexpectedCurrencyError : IError
{
    public string Message => "Unexpected currency given!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
