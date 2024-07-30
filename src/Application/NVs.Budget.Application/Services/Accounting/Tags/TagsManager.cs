using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Tags;

internal class TagsManager(IReadOnlyCollection<TaggingCriterion> criteria)
{
    public IReadOnlyCollection<Tag> GetTags(TrackedOperation operation)
    {
        return criteria.Where(c => c.Criterion(operation)).Select(c => c.Tag(operation)).ToList().AsReadOnly();
    }
}
