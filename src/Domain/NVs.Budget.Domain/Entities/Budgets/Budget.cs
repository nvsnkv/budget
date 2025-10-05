namespace NVs.Budget.Domain.Entities.Budgets;

public class Budget : EntityBase<Guid>
{
    private readonly List<Owner> _owners = new();
    public string Name { get; private set; }

    public Budget(Guid id, string name, IEnumerable<Owner> owners) : base(id)
    {
        Name = name;

        _owners.AddRange(owners.DistinctBy(o => o.Id));
        if (_owners.Count == 0)
        {
            throw new ArgumentException("Unable to create budget without owners!");
        }
    }

    public void Rename(string name)
    {
        Name = name;
    }

    public IReadOnlyCollection<Owner> Owners => _owners.AsReadOnly();

    public void AddOwner(Owner owner)
    {
        if (!_owners.Contains(owner))
        {
            _owners.Add(owner);
        }
    }

    public void RemoveOwner(Owner owner)
    {
        if (!_owners.Any(owner.Equals)) return;
        if (_owners.Count == 1)
        {
            throw new InvalidOperationException("Unable to remove last owner of a budget!");
        }

        _owners.Remove(owner);
    }
}
