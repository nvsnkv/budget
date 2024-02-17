using FluentResults;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class UnableToTagTransferError(IEnumerable<IError> reasons) : IError
{
    public string Message => "Unable to tag a transaction which is a part of a transfer!";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new(reasons);
}
