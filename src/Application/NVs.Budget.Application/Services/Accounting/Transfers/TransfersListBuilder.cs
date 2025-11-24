using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

internal class TransfersListBuilder(TransferDetector detector, DetectionAccuracy? tagIf)
{
    private readonly List<TrackedOperation> _parts = new();

    public TrackedTransfer? Add(TrackedOperation operation)
    {
        foreach (var part in _parts.Where(p => Math.Sign(p.Amount.Amount) != Math.Sign(operation.Amount.Amount)))
        {
            var (source, sink) = part.Amount.Amount > operation.Amount.Amount
                ? (operation, part)
                : (part, operation);

            var detectionResult = detector.Detect(source, sink);

            if (detectionResult.IsSuccess)
            {
                _parts.Remove(part);

                if (tagIf.HasValue && detectionResult.Value.Accuracy >= tagIf.Value)
                {
                    source.Tag(TransferTags.Transfer);
                    source.Tag(TransferTags.Source);

                    sink.Tag(TransferTags.Transfer);
                    sink.Tag(TransferTags.Sink);
                }

                return detectionResult.Value;
            }
        }

        _parts.Add(operation);

        return null;
    }
}
