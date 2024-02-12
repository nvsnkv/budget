using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Errors;

public class NoTransferCriteriaMatchedError: IError
{
    public string Message => "No transfer criteria matched for this pair!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
