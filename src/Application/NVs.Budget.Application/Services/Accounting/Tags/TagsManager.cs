using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Services.Accounting.Tags;

internal class TagsManager
{
    private static readonly ExpressionParser Parser = new();
    private readonly IReadOnlyCollection<TaggingCriterion> _criteria;

    public TagsManager(IEnumerable<TaggingRule> rules)
    {
        _criteria = rules
            .Select(r => new TaggingCriterion(
                o => new Tag(Parser.ParseConversion<TrackedOperation, string>(r.Tag, "o").Compile()(o)),
                Parser.ParsePredicate<TrackedOperation>(r.Condition, "o").Compile()))
            .ToList()
            .AsReadOnly();
    }

    public IReadOnlyCollection<Tag> GetTags(TrackedOperation operation)
    {
        return _criteria.Where(c => c.Criterion(operation)).Select(c => c.Tag(operation)).ToList().AsReadOnly();
    }
}
