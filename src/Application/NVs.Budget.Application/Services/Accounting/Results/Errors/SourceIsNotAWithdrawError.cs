using FluentResults;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class SourceIsNotAWithdrawError(Operation source): IError
{
    public string Message => "Given source is not a withdraw!";
    public Dictionary<string, object> Metadata { get; } = new() { { "Source", source.Id } };
    public List<IError> Reasons { get; } = new();
}
