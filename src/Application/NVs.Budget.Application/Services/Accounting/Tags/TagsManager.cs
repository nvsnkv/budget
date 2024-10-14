using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Services.Accounting.Tags;

internal class TagsManager
{
    private static readonly ExpressionParser Parser = new();
    private readonly IReadOnlyCollection<TaggingCriterion> _criteria;

    public TagsManager(IEnumerable<TaggingCriterion> criteria)
    {
        _criteria = criteria
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Tag> GetTagsFor(TrackedOperation operation)
    {
        return _criteria.Where(c => c.Condition.AsInvokable()(operation))
            .Select(c => new Tag(c.Tag.AsInvokable()(operation)))
            .ToList()
            .AsReadOnly();
    }
}
