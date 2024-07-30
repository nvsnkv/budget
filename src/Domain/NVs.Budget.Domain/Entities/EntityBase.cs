using System.Diagnostics;

namespace NVs.Budget.Domain.Entities;

[DebuggerDisplay("{GetType().Name}: {Id}")]
public abstract class EntityBase<T> where T: struct
{
    protected EntityBase(T id)
    {
        Id = id;
    }

    public T Id { get; }

    public static bool operator ==(EntityBase<T>? left, EntityBase<T>? right)
    {
        return left?.Equals((object?)right) ?? right is null;
    }

    public static bool operator !=(EntityBase<T>? left, EntityBase<T>? right)
    {
        return !(left == right);
    }

    protected bool Equals(EntityBase<T> other)
    {
        return EqualityComparer<T>.Default.Equals(Id, other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj.GetType().IsAssignableTo(this.GetType()) && Equals((EntityBase<T>)obj);
    }

    public override int GetHashCode()
    {
        return EqualityComparer<T>.Default.GetHashCode(Id);
    }
}
