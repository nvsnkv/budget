using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

internal class TransferDetector(IReadOnlyCollection<TransferCriteria> criteria)
{
    public Result<TrackedTransfer> Detect(TrackedTransaction source, TrackedTransaction sink)
    {

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
