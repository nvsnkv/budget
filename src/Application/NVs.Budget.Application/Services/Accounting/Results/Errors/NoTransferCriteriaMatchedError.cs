using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class NoTransferCriteriaMatchedError: IError
{
    public string Message => "No transfer criteria matched for this pair!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}
