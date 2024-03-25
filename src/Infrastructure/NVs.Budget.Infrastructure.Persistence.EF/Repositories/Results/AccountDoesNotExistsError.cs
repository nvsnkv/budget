using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class AccountDoesNotExistsError(Account account)
    : ErrorBase("Account with given id does not exists",
        new Dictionary<string, object>()
        {
            { nameof(TrackedAccount.Id), account.Id }
        }
    );
