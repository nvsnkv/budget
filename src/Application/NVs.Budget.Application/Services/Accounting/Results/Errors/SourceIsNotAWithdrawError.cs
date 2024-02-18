using FluentResults;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Services.Accounting.Results.Errors;

internal class SourceIsNotAWithdrawError(Transaction source): IError
{
    public string Message => "Given source is not a withdraw!";
    public Dictionary<string, object> Metadata { get; } = new() { { "Source", source.Id } };
    public List<IError> Reasons { get; } = new();
}
