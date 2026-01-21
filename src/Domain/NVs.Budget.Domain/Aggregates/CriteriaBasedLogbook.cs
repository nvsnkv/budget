using FluentResults;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Errors;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Aggregates;

public class CriteriaBasedLogbook : Logbook
{
    private readonly Dictionary<Criterion, CriteriaBasedLogbook> _children;

    public CriteriaBasedLogbook(Criterion criterion)
    {
        Criterion = criterion;
        _children = criterion.Subcriteria
            .ToDictionary(c => c, c => new CriteriaBasedLogbook(c));
    }

    public Criterion Criterion { get; }

    public IReadOnlyDictionary<Criterion, CriteriaBasedLogbook> Children => _children.AsReadOnly();

    public override Result Register(Operation o)
    {
        if (!Criterion.Matched(o))
            return Result.Fail(new OperationDidNotMatchCriteriaError()
                .WithOperationId(o)
                .WithMetadata(nameof(Criterion), Criterion)
            );

        var childResult = Result.Ok();
        if (Criterion.Subcriteria.Any())
        {
            var subcriterion = Criterion.GetMatchedSubcriterion(o);
            if (subcriterion is null)
                return Result.Fail(new OperationDidNotMatchSubcriteriaError().WithOperationId(o)
                    .WithMetadata(nameof(Criterion), Criterion)
                );

            if (!_children.ContainsKey(subcriterion))
            {
                _children.Add(subcriterion, new CriteriaBasedLogbook(subcriterion));
            }

            childResult = _children[subcriterion].Register(o);
        }

        return childResult.IsSuccess ? base.Register(o) : childResult;
    }

    protected override Logbook CreateSubRangedLogbook() => new CriteriaBasedLogbook(Criterion);

    public override Logbook this[DateTime from, DateTime till]
    {
        get
        {
            if (IsEmpty)
            {
                return new CriteriaBasedLogbook(Criterion);
            }

            return base[from, till];
        }
    }
}
