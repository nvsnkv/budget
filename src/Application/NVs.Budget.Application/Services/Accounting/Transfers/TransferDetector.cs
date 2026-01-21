using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

internal class TransferDetector(IReadOnlyList<TransferCriterion> criteria)
{
   public Result<TrackedTransfer> Detect(TrackedOperation source, TrackedOperation sink)
    {
        if (source.Amount.Amount >= 0) return Result.Fail(new SourceIsNotAWithdrawError(source));
        if (sink.Amount.Amount <= 0) return Result.Fail(new SinkIsNotAnIncomeError(sink));
        if (!source.Amount.HasSameCurrencyAs(sink.Amount)) return Result.Fail(new SourceAndSinkHaveDifferentCurrenciesError(source, sink));

        TransferCriterion? bestMatch = null;
        foreach (var criterion in criteria)
        {
            if (criterion.Criterion.AsInvokable()(source, sink))
            {
                if (bestMatch is null || bestMatch.Accuracy < criterion.Accuracy)
                {
                    bestMatch = criterion;
                }
            }
        }

        return bestMatch is not null
            ? new TrackedTransfer(source, sink, bestMatch.Comment)
            {
                Accuracy = bestMatch.Accuracy
            }
            : Result.Fail(new NoTransferCriteriaMatchedError()
                .WithMetadata(nameof(source), source.Id)
                .WithMetadata(nameof(sink), sink.Id));
    }
}
