using FluentResults;
using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Results;

public class ImportResult : Result
{
    public ImportResult(
        IReadOnlyCollection<TrackedTransaction> transactions,
        IReadOnlyCollection<TrackedTransfer> transfers,
        IReadOnlyCollection<IReadOnlyCollection<TrackedTransaction>> duplicates,
        IEnumerable<IReason> reasons)
    {
        Transactions = transactions;
        Transfers = transfers;
        Duplicates = duplicates;
        Reasons.AddRange(reasons);
    }

    public IReadOnlyCollection<TrackedTransaction> Transactions { get; }

    public IReadOnlyCollection<TrackedTransfer> Transfers { get; }

    public IReadOnlyCollection<IReadOnlyCollection<TrackedTransaction>> Duplicates { get; }
}
