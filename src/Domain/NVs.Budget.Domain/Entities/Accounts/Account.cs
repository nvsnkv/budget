namespace NVs.Budget.Domain.Entities.Accounts;

public class Account : EntityBase<Guid>
{
    private readonly List<Owner> _owners = new();
    public string Name { get; private set; }
    public string Bank { get; private set; }

    public Account(Guid id, string name, string bank, IEnumerable<Owner> owners) : base(id)
    {
        Name = name;
        Bank = bank;

        _owners.AddRange(owners.DistinctBy(o => o.Id));
        if (_owners.Count == 0)
        {
            throw new ArgumentException("Unable to create account without owners!");
        }
    }

    public void Rename(string name, string bank)
    {
        Name = name;
        Bank = bank;
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
            throw new InvalidOperationException("Unable to remove last owner of account!");
        }

        _owners.Remove(owner);
    }
}
