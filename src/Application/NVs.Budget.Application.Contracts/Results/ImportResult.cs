using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.Results;

public class ImportResult : Result
{
    public ImportResult(
        IReadOnlyCollection<TrackedOperation> operations,
        IReadOnlyCollection<TrackedTransfer> transfers,
        IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>> duplicates,
        IEnumerable<IReason> reasons)
    {
        Operations = operations;
        Transfers = transfers;
        Duplicates = duplicates;
        Reasons.AddRange(reasons);
    }

    public IReadOnlyCollection<TrackedOperation> Operations { get; }

    public IReadOnlyCollection<TrackedTransfer> Transfers { get; }

    public IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>> Duplicates { get; }
}
