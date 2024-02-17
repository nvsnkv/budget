using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Tags;

public class TagsManager(IReadOnlyCollection<TaggingCriterion> criteria)
{
    public IReadOnlyCollection<Tag> GetTags(TrackedTransaction transaction)
    {
        return criteria.Where(c => c.Criterion(transaction)).Select(c => c.Tag).ToList().AsReadOnly();
    }
}
