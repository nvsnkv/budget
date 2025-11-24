namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public sealed class TransfersList 
{   
    private readonly List<TrackedTransfer> _recorded = new();
    private readonly List<UnregisteredTransfer> _unregistereds = new();

    public IReadOnlyCollection<TrackedTransfer> Recorded => _recorded.AsReadOnly();
    public IReadOnlyCollection<UnregisteredTransfer> Unregistereds => _unregistereds.AsReadOnly();

    public void Add(TrackedTransfer transfer)
    {
        _recorded.Add(transfer);
    }

    public void Add(UnregisteredTransfer transfer)
    {
        _unregistereds.Add(transfer);
    }
}