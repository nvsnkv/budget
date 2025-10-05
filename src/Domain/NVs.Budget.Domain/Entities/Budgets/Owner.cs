namespace NVs.Budget.Domain.Entities.Budgets;

public class Owner : EntityBase<Guid>
{
    public static readonly Owner Invalid = new(Guid.Empty, string.Empty);

    public string Name { get; }

    public Owner(Guid id, string name) : base(id)
    {
        Name = name;
    }
}
