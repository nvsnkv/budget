using System.Collections.ObjectModel;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting;

public class TagsManager(IReadOnlyCollection<TaggingCriterion> criteria)
{
    public IReadOnlyCollection<Tag> GetTags(TrackedTransaction transaction)
    {
        return criteria.Where(c => c.Criterion(transaction)).Select(c => c.Tag).ToList().AsReadOnly();
    }
}

public record TaggingCriterion(Tag Tag, Func<TrackedTransaction, bool> Criterion);
