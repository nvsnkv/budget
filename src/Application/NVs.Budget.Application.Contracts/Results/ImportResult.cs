using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Application.Contracts.Results;

public class ImportResult(
    IReadOnlyCollection<TrackedOperation> operations,
    IReadOnlyCollection<TrackedTransfer> transfers,
    IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>> duplicates,
    IEnumerable<IReason> reasons)
    : UpdateResult(operations, transfers, reasons)
{
    public IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>> Duplicates { get; } = duplicates;
}
