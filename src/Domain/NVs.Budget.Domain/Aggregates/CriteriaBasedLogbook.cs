using FluentResults;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Errors;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Aggregates;

public class CriteriaBasedLogbook : Logbook
{
    public CriteriaBasedLogbook(Criterion criterion)
    {
        Criterion = criterion;
        Children = criterion.Subcriteria
            .ToDictionary(c => c, c => new CriteriaBasedLogbook(c))
            .AsReadOnly();
    }

    public Criterion Criterion { get; }

    public IReadOnlyDictionary<Criterion, CriteriaBasedLogbook> Children { get; }

    public override Result Register(Operation o)
    {
        if (!Criterion.Matched(o))
            return Result.Fail(new OperationDidNotMatchCriteriaError()
                .WithTransactionId(o)
                .WithMetadata(nameof(Criterion), Criterion)
            );

        var childResult = Result.Ok();
        if (Criterion.Subcriteria.Any())
        {
            var subcriterion = Criterion.GetMatchedSubcriterion(o);
            if (subcriterion is null)
                return Result.Fail(new OperationDidNotMatchSubcriteriaError().WithTransactionId(o)
                    .WithMetadata(nameof(Criterion), Criterion)
                );

            childResult = Children[subcriterion].Register(o);
        }

        return childResult.IsSuccess ? base.Register(o) : childResult;
    }

    protected override Logbook CreateSubRangedLogbook() => new CriteriaBasedLogbook(Criterion);
}
