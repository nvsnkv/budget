using NMoneys;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Entities.Accounting;

public class TrackedTransfer : Transfer
{
    public TrackedTransfer(Transaction source, Transaction sink, string comment) : base(source, sink, comment)
    {
    }

    public TrackedTransfer(Transaction source, Transaction sink, Money fee, string comment) : base(source, sink, fee, comment)
    {
    }

    public DetectionAccuracy Accuracy { get; init; }
}
