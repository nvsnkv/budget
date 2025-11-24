using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Contracts.Results;

public class UpdateResult : Result
{
    public UpdateResult(IReadOnlyCollection<TrackedOperation> operations, IReadOnlyCollection<TrackedTransfer> transfers, IEnumerable<IReason> reasons)
    {
        Operations = operations;
        Transfers = transfers;
        Reasons.AddRange(reasons);
    }

    public IReadOnlyCollection<TrackedOperation> Operations { get; }
    public IReadOnlyCollection<TrackedTransfer> Transfers { get; }
}