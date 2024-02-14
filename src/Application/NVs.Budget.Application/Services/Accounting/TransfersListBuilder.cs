using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Services.Accounting;

internal class TransfersListBuilder(TransferDetector detector)
{
    private readonly List<TrackedTransfer> _transfers = new();
    private readonly List<TrackedTransaction> _parts = new();

    public void Clear()
    {
        _transfers.Clear();
        _parts.Clear();
    }

    public IReadOnlyCollection<TrackedTransfer> ToList() => _transfers.AsReadOnly();

    public void Add(TrackedTransaction transaction)
    {
        var transferDetected = false;
        foreach (var part in _parts.Where(p => Math.Sign(p.Amount.Amount) != Math.Sign(transaction.Amount.Amount)))
        {
            var (source, sink) = part.Amount.Amount > transaction.Amount.Amount
                ? (transaction, part)
                : (part, transaction);

            var detectionResult = detector.Detect(source, sink);

            if (detectionResult.IsSuccess)
            {
                _transfers.Add(detectionResult.Value);
                _parts.Remove(source);
                transferDetected = true;
                break;
            }
        }

        if (!transferDetected)
        {
            _parts.Add(transaction);
        }

    }
}