using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Contracts.Criteria;

public class LogbookCriteria(
    string description,
    IReadOnlyCollection<LogbookCriteria>? subcriteria,
    TagBasedCriterionType? type,
    IReadOnlyCollection<Tag>? tags,
    ReadableExpression<Func<Operation, string>>? substitution,
    ReadableExpression<Func<Operation, bool>>? criteria)
{
    public static readonly LogbookCriteria Universal = new(string.Empty, null, null, null, null, null);

    public string Description { get; } = description;
    public IReadOnlyCollection<LogbookCriteria>? Subcriteria { get; } = subcriteria;
    public TagBasedCriterionType? Type { get; } = type;
    public IReadOnlyCollection<Tag>? Tags { get; } = tags;
    public ReadableExpression<Func<Operation, string>>? Substitution { get; } = substitution;
    public ReadableExpression<Func<Operation, bool>>? Criteria { get; } = criteria;

    public Criterion GetCriterion()
    {
        var subcriteria = Subcriteria?.Select(s => s.GetCriterion());
        if (Criteria is not null)
        {
            return subcriteria is not null
                ? new PredicateBasedCriterion(Description, Criteria, subcriteria)
                : new PredicateBasedCriterion(Description, Criteria);
        }

        if (Substitution is not null)
        {
            return new SubstitutionBasedCriterion(Description, Substitution);
        }

        if (Tags is not null && Type.HasValue)
        {
            return subcriteria is not null
                ? new TagBasedCriterion(Description, Tags, Type.Value, subcriteria)
                : new TagBasedCriterion(Description, Tags, Type.Value);
        }

        return subcriteria is not null
            ? new UniversalCriterion(Description, subcriteria)
            : new UniversalCriterion(Description);
    }

    public override string ToString() => $"{GetType().Name}: {Description}";
}
