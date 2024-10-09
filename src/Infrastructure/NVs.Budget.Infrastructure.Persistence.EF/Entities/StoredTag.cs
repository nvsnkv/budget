using JetBrains.Annotations;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredTag(string value)
{
    public string Value { get; [UsedImplicitly] private set; } = value;

    private bool Equals(StoredTag other) => Value == other.Value;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((StoredTag)obj);
    }

    public override int GetHashCode() => Value.GetHashCode();
}
