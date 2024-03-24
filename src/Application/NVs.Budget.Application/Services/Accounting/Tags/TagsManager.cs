using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Tags;

internal class TagsManager(IReadOnlyCollection<TaggingCriterion> criteria)
{
    public IReadOnlyCollection<Tag> GetTags(TrackedOperation operation)
    {
        return criteria.Where(c => c.Criterion(operation)).Select(c => c.Tag).ToList().AsReadOnly();
    }
}
