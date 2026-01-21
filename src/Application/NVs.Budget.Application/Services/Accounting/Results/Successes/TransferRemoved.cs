using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransferRemoved : Success
{
    public TransferRemoved(TrackedTransfer transfer):base("Transfer successfully removed!")
    {
        Metadata.Add(nameof(transfer.Source), transfer.Source.Id);
        Metadata.Add(nameof(transfer.Sink), transfer.Sink.Id);
    }
}