using FluentResults;
using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransferAdded : Success
{
    public TransferAdded(TrackedTransfer transfer, string message = "Transfer was successfully added!") : base(message)
    {
        WithMetadata(nameof(transfer.Source), transfer.Source.Id)
            .WithMetadata(nameof(transfer.Sink), transfer.Sink.Id)
            .WithMetadata(nameof(transfer.Accuracy), transfer.Accuracy);
    }
}
