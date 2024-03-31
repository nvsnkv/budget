namespace NVs.Budget.Domain.Entities.Accounts;

public class Owner : EntityBase<Guid>
{
    public string Name { get; }

    public Owner(Guid id, string name) : base(id)
    {
        Name = name;
    }
}
