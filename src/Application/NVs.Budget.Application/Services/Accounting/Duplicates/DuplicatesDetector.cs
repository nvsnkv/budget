using System.Collections.ObjectModel;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;

namespace NVs.Budget.Application.Services.Accounting.Duplicates;

internal class DuplicatesDetector(DuplicatesDetectorOptions options)
{
    private List<List<TrackedOperation>> _buckets = new();

    public async ValueTask<IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>> DetectDuplicates(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct)
    {
        _buckets.Clear();
        await foreach (var transaction in operations.WithCancellation(ct))
        {
            PlaceOperation(transaction);
        }

        return BuildDuplicatesLists();
    }

    public IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>> DetectDuplicates(IEnumerable<TrackedOperation> operations)
    {
        _buckets.Clear();
        foreach (var transaction in operations)
        {
            PlaceOperation(transaction);
        }

        return BuildDuplicatesLists();
    }

    private ReadOnlyCollection<ReadOnlyCollection<TrackedOperation>> BuildDuplicatesLists()
    {
        return _buckets.Where(d => d.Count > 1)
            .Select(d => d.AsReadOnly())
            .ToList()
            .AsReadOnly();
    }

    private void PlaceOperation(TrackedOperation operation)
    {
        var duplicateFound = false;
        foreach (var bucket in _buckets.Where(b => CheckIsDuplicate(b.First(), operation)))
        {
            bucket.Add(operation);
            break;
        }

        if (!duplicateFound)
        {
            _buckets.Add(new List<TrackedOperation> { operation });
        }
    }

    private bool CheckIsDuplicate(TrackedOperation left, TrackedOperation right) =>
        left.Budget == right.Budget
        && left.Amount == right.Amount
        && left.Description == right.Description
        && left.Timestamp - right.Timestamp < options.Offset;
}
