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

    public override Result Register(Operation t)
    {
        if (!Criterion.Matched(t))
            return Result.Fail(new OperationDidNotMatchCriteriaError()
                .WithTransactionId(t)
                .WithMetadata(nameof(Criterion), Criterion)
            );

        var childResult = Result.Ok();
        if (Criterion.Subcriteria.Any())
        {
            var subcriterion = Criterion.GetMatchedSubcriterion(t);
            if (subcriterion is null)
                return Result.Fail(new OperationDidNotMatchSubcriteriaError().WithTransactionId(t)
                    .WithMetadata(nameof(Criterion), Criterion)
                );

            childResult = Children[subcriterion].Register(t);
        }

        return childResult.IsSuccess ? base.Register(t) : childResult;
    }

    protected override Logbook CreateSubRangedLogbook() => new CriteriaBasedLogbook(Criterion);
}
