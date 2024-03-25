using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class CannotChangeAccountError(TrackedOperation t) :
    ErrorBase("Cannot update account for existing transaction: operation is not supported", new()
    {
        {"TransactionId", t.Id }
    });
