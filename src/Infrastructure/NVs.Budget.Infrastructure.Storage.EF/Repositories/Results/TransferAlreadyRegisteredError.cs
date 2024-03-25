using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;

namespace NVs.Budget.Infrastructure.Storage.Repositories.Results;

internal class TransferAlreadyRegisteredError(TrackedTransfer transfer) : ErrorBase("Transfer is already registered!", new()
{
    { nameof(Transfer.Source), transfer.Source.Id },
    { nameof(Transfer.Sink), transfer.Sink.Id }
});
