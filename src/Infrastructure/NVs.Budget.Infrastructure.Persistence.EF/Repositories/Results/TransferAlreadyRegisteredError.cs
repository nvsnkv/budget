using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

internal class TransferAlreadyRegisteredError(TrackedTransfer transfer) : ErrorBase("Transfer is already registered!", new()
{
    { nameof(Transfer.Source), transfer.Source.Id },
    { nameof(Transfer.Sink), transfer.Sink.Id }
});
