using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

internal class TransferDetector(IReadOnlyList<TransferCriterion> criteria)
{
    public Result<TrackedTransfer> Detect(TrackedTransaction source, TrackedTransaction sink)
    {
        if (source.Amount.Amount >= 0) return Result.Fail(new SourceIsNotAWithdrawError(source));
        if (sink.Amount.Amount <= 0) return Result.Fail(new SinkIsNotAnIncomeError(sink));
        if (!source.Amount.HasSameCurrencyAs(sink.Amount)) return Result.Fail(new SourceAndSinkHaveDifferentCurrenciesError(source, sink));

        foreach (var criterion in criteria)
        {
            if (criterion.Criterion(source, sink))
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
