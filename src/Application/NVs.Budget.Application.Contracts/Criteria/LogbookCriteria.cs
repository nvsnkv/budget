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
    ReadableExpression<Func<Operation, bool>>? criteria,
    bool? isUniversal,
    ReadableExpression<Func<Operation, bool>>? precondition = null)
{
    public static readonly LogbookCriteria Universal = new(string.Empty, null, null, null, null, null, true);

    public string Description { get; } = description;
    public IReadOnlyCollection<LogbookCriteria>? Subcriteria { get; } = subcriteria;
    public TagBasedCriterionType? Type { get; } = type;
    public IReadOnlyCollection<Tag>? Tags { get; } = tags;
    public ReadableExpression<Func<Operation, string>>? Substitution { get; } = substitution;
    public ReadableExpression<Func<Operation, bool>>? Criteria { get; } = criteria;
    public bool? IsUniversal { get; } = isUniversal;
    public ReadableExpression<Func<Operation, bool>>? Precondition { get; } = precondition;

    public Criterion GetCriterion()
    {
        var subcriteria = Subcriteria?.Select(s => s.GetCriterion());
        var precondition = Precondition is null ? (Func<Operation, bool>)(_ => true) : Precondition;

        if (Criteria is not null)
        {
            return subcriteria is not null
                ? new PredicateBasedCriterion(Description, Criteria, subcriteria, precondition)
                : new PredicateBasedCriterion(Description, Criteria, precondition);
        }

        if (Substitution is not null)
        {
            return new SubstitutionBasedCriterion(Description, Substitution, precondition);
        }

        if (Tags is not null && Type.HasValue)
        {
            return subcriteria is not null
                ? new TagBasedCriterion(Description, Tags, Type.Value, subcriteria, precondition)
                : new TagBasedCriterion(Description, Tags, Type.Value, precondition);
        }

        if (subcriteria is not null)
        {
            return IsUniversal.HasValue && IsUniversal.Value
                ? new UniversalCriterion(Description, subcriteria, precondition)
                : new SubcriteriaDrivenCriterion(Description, subcriteria, precondition);
        }

        return new UniversalCriterion(Description, precondition);
    }

    public override string ToString() => $"{GetType().Name}: {Description}";
}
