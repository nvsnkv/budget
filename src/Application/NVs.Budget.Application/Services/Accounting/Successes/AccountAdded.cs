using FluentResults;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Successes;

internal class AccountAdded : Success
{
    public AccountAdded(Account account) : base("Account was successfully added!")
    {
        this.WithAccountId(account);
    }
}