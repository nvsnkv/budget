namespace NVs.Budget.Application.Contracts.Entities.Accounting;

public sealed class TransfersList 
{   
    private readonly List<TrackedTransfer> _recorded = new();
    private readonly List<UnregisteredTransfer> _unregistered = new();

    public IReadOnlyCollection<TrackedTransfer> Recorded => _recorded.AsReadOnly();
    public IReadOnlyCollection<UnregisteredTransfer> Unregistered => _unregistered.AsReadOnly();

    public void Add(TrackedTransfer transfer)
    {
        _recorded.Add(transfer);
    }

    public void Add(UnregisteredTransfer transfer)
    {
        _unregistered.Add(transfer);
    }
}