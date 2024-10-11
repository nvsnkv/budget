using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

internal class TransferDetector(IReadOnlyList<TransferCriterion> criteria)
{
    private static readonly TransferCriteriaParser Parser = new();

    private readonly Dictionary<TransferCriterion, Func<TrackedOperation, TrackedOperation, bool>> _functions = criteria.ToDictionary(
        c => c,
        c => Parser.ParseTransferCriteria(c.Criterion).Compile()
    );

    public Result<TrackedTransfer> Detect(TrackedOperation source, TrackedOperation sink)
    {
        if (source.Amount.Amount >= 0) return Result.Fail(new SourceIsNotAWithdrawError(source));
        if (sink.Amount.Amount <= 0) return Result.Fail(new SinkIsNotAnIncomeError(sink));
        if (!source.Amount.HasSameCurrencyAs(sink.Amount)) return Result.Fail(new SourceAndSinkHaveDifferentCurrenciesError(source, sink));

        foreach (var criterion in _functions.Keys)
        {
            if (_functions[criterion](source, sink))
            {
                return new TrackedTransfer(source, sink, criterion.Comment)
                {
                    Accuracy = criterion.Accuracy
                };
            }
        }

        return Result.Fail(new NoTransferCriteriaMatchedError()
            .WithMetadata(nameof(source), source.Id)
            .WithMetadata(nameof(sink), sink.Id));
    }
}

internal class TransferCriteriaParser : ExpressionParser
{
    public Expression<Func<TrackedOperation, TrackedOperation, bool>> ParseTransferCriteria(string expression) =>
        Parse<Func<TrackedOperation, TrackedOperation, bool>>(
            expression,
            typeof(bool),
            Expression.Parameter(typeof(TrackedOperation), "l"),
            Expression.Parameter(typeof(TrackedOperation), "r")
        );
}
