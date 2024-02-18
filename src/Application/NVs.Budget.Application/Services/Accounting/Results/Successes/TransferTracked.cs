using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Results.Successes;

internal class TransferTracked : TransferAdded
{
    public TransferTracked(TrackedTransfer transfer) : base(transfer, "Transfer was successfully stored!")
    {
    }
}