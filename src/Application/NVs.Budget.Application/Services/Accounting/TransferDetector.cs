using FluentResults;
using NVs.Budget.Application.Services.Accounting.Errors;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting;

internal class TransferDetector(IReadOnlyCollection<TransferCriteria> criteria, ITransfersRepository repository)
{
    public async Task<Result<TrackedTransfer>> Detect(TrackedTransaction source, TrackedTransaction sink, CancellationToken ct)
    {
        var transfer = await repository.GetTransfer(source, sink, ct);
        if (transfer is not null)
        {
            return transfer;
        }

        foreach (var criterion in criteria)
        {
            if (criterion.Criterion(source, sink))
            {
                transfer = new TrackedTransfer(source, sink, criterion.Comment)
                {
                    Accuracy = criterion.Accuracy
                };

                if (transfer.Accuracy == DetectionAccuracy.Exact)
                {
                    var result = await repository.Track(transfer, ct);
                    if (!result.IsSuccess) return result.ToResult<TrackedTransfer>();
                }

                return transfer;
            }
        }

        return Result.Fail(new NoTransferCriteriaMatchedError()
            .WithMetadata(nameof(source), source.Id)
            .WithMetadata(nameof(sink), sink.Id));
    }

}

public record TransferCriteria(
    DetectionAccuracy Accuracy,
    string Comment,
    Func<TrackedTransaction, TrackedTransaction, bool> Criterion);

public enum DetectionAccuracy
{
    Exact,
    Likely
}
