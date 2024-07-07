using FluentResults;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class AccountNotFoundError(Guid accountId) : IError
{
    public string Message { get; } = "Account not found!";

    public Dictionary<string, object> Metadata { get; } = new()
    {
        { $"{nameof(Account)}.{nameof(Account.Id)}", accountId }
    };

    public List<IError> Reasons { get; } = new();
}