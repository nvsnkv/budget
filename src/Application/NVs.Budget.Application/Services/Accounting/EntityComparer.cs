using NVs.Budget.Domain.Entities;

namespace NVs.Budget.Application.Services.Accounting
{
    internal class EntityComparer<T> : IEqualityComparer<T> where T : EntityBase<Guid>
    {
        public static readonly EntityComparer<T> Instance = new();

        public bool Equals(T? x, T? y) => x?.Id == y?.Id;
        public int GetHashCode(T obj) => obj.Id.GetHashCode();
    }
}