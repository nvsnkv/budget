using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Storage.Repositories.Results;

internal class AccountDoesNotExistsError(TrackedAccount account)
    : ErrorBase("Account with given id does not exists",
        new Dictionary<string, object>()
        {
            { nameof(TrackedAccount.Id), account.Id }
        }
    );
