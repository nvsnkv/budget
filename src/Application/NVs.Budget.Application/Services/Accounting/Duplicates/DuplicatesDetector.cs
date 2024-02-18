using NVs.Budget.Application.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Duplicates;

internal class DuplicatesDetector(DuplicatesDetectorSettings settings)
{
    public IReadOnlyCollection<IReadOnlyCollection<TrackedTransaction>> DetectDuplicates(IEnumerable<TrackedTransaction> transactions)
    {
        var buckets = new List<List<TrackedTransaction>>();
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
                buckets.Add(new List<TrackedTransaction>() { transaction });
            }
        }

        return buckets.Where(d => d.Count > 1)
            .Select(d => d.AsReadOnly())
            .ToList()
            .AsReadOnly();
    }

    private bool CheckIsDuplicate(TrackedTransaction left, TrackedTransaction right) =>
        left.Account == right.Account
        && left.Amount == right.Amount
        && left.Description == right.Description
        && left.Timestamp - right.Timestamp < settings.Offset;
}
