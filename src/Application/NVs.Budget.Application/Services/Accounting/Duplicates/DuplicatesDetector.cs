using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;

namespace NVs.Budget.Application.Services.Accounting.Duplicates;

internal class DuplicatesDetector(DuplicatesDetectorOptions options)
{
    public IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>> DetectDuplicates(IEnumerable<TrackedOperation> transactions)
    {
        var buckets = new List<List<TrackedOperation>>();
        foreach (var transaction in transactions)
        {
            var duplicateFound = false;
            foreach (var bucket in buckets.Where(b => CheckIsDuplicate(b.First(), transaction)))
            {
                bucket.Add(transaction);
                break;
            }

            if (!duplicateFound)
            {
                buckets.Add(new List<TrackedOperation>() { transaction });
            }
        }

        return buckets.Where(d => d.Count > 1)
            .Select(d => d.AsReadOnly())
            .ToList()
            .AsReadOnly();
    }

    private bool CheckIsDuplicate(TrackedOperation left, TrackedOperation right) =>
        left.Budget == right.Budget
        && left.Amount == right.Amount
        && left.Description == right.Description
        && left.Timestamp - right.Timestamp < options.Offset;
}
