using FluentResults;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class AccountNotFoundError(Guid accountId) : IError
{
    public string Message { get; } = "Account not found!";

    public Dictionary<string, object> Metadata { get; } = new()
    {
        { $"{nameof(Domain.Entities.Accounts.Budget)}.{nameof(Domain.Entities.Accounts.Budget.Id)}", accountId }
    };

    public List<IError> Reasons { get; } = new();
}