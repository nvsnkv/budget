using NMoneys;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Contracts.Entities.Budgeting;

public class TrackedTransfer : Transfer
{
    public TrackedTransfer(Operation source, Operation sink, string comment) : base(source, sink, comment)
    {
    }

    public TrackedTransfer(Operation source, Operation sink, Money fee, string comment) : base(source, sink, fee, comment)
    {
    }

    public DetectionAccuracy Accuracy { get; init; }
}
