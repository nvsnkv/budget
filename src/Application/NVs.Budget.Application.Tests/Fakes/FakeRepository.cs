using System.Linq.Expressions;
using FluentResults;

namespace NVs.Budget.Application.Tests.Fakes;

internal class FakeRepository<T>
{
    protected readonly List<T> Data = new();

    public Task<IReadOnlyCollection<T>> Get(Expression<Func<T, bool>> filter, CancellationToken ct)
    {
        var predicate = filter.Compile();
        var result = Data.Where(predicate).ToList().AsReadOnly();
        return Task.FromResult((IReadOnlyCollection<T>)result);
    }


    public Task<Result<T>> Update(T item, CancellationToken ct)
    {
        var target = Data.First(a => a?.Equals(item) ?? false);
        Data.Remove(target);
        Data.Add(item);

        return Task.FromResult<Result<T>>(item);
    }

    public Task<Result> Remove(T item, CancellationToken ct)
    {
        var target = Data.First(a => a?.Equals(item) ?? false);
        Data.Remove(target);

        return Task.FromResult(Result.Ok());
    }

    public void Append(IEnumerable<T> items)
    {
        Data.AddRange(items);
    }
}
