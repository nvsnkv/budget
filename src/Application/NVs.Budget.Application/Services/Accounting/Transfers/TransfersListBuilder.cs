using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Transfers;

internal class TransfersListBuilder(TransferDetector detector)
{
    private readonly List<TrackedTransfer> _transfers = new();
    private readonly List<TrackedOperation> _parts = new();

    public void Clear()
    {
        _transfers.Clear();
        _parts.Clear();
    }

    public IReadOnlyCollection<TrackedTransfer> ToList() => _transfers.AsReadOnly();

    public void Add(TrackedOperation operation)
    {
        var transferDetected = false;
        foreach (var part in _parts.Where(p => Math.Sign(p.Amount.Amount) != Math.Sign(operation.Amount.Amount)))
        {
            var (source, sink) = part.Amount.Amount > operation.Amount.Amount
                ? (operation, part)
                : (part, operation);

            var detectionResult = detector.Detect(source, sink);

            if (detectionResult.IsSuccess)
            {
                _transfers.Add(detectionResult.Value);
                _parts.Remove(source);
                transferDetected = true;

                source.Tag(TransferTags.Transfer);
                source.Tag(TransferTags.Source);

                sink.Tag(TransferTags.Transfer);
                sink.Tag(TransferTags.Sink);
                break;
            }
        }

        if (!transferDetected)
        {
            _parts.Add(operation);
        }

    }
}
