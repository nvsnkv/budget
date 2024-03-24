using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Storage.Repositories.Results;

internal class CannotChangeAccountError(TrackedOperation t) :
    ErrorBase("Cannot update account for existing transaction: operation is not supported", new()
    {
        {"TransactionId", t.Id }
    });
