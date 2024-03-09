using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Entities;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Infrastructure.Storage.Entities;
using NVs.Budget.Infrastructure.Storage.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal abstract class RepositoryBase<TItem, TKey, TRecord>(IMapper mapper, VersionGenerator generator)
    where TRecord : DbRecord, ITrackableEntity<TKey>
    where TItem : ITrackableEntity<TKey>
    where TKey : struct
{
    protected readonly IMapper Mapper = mapper;
    public async Task<IReadOnlyCollection<TItem>> Get(Expression<Func<TItem, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TItem, TRecord>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var items = await GetData(expression).AsNoTracking().ProjectTo<TItem>(Mapper.ConfigurationProvider).ToListAsync(ct);
        return items;
    }

    public async Task<Result<TItem>> Update(TItem item, CancellationToken ct)
    {
        var target = await GetTarget(item, ct);
        if (target is null) return Result.Fail(new EntityDoesNotExistError<TItem>(item));
        if (item.Version != target.Version) return Result.Fail(new VersionDoesNotMatchError<TItem, TKey>(item));

        BumpVersion(target);
        var updated = await Update(target, item, ct);
        return updated.IsSuccess
            ? Result.Ok(Mapper.Map<TItem>(updated.Value))
            : Result.Fail(updated.Errors);
    }

    public async Task<Result> Remove(TItem item, CancellationToken ct)
    {
        var target = await GetTarget(item, ct);
        if (target is null) return Result.Fail(new EntityDoesNotExistError<TItem>(item));
        if (item.Version != target.Version) Result.Fail(new VersionDoesNotMatchError<TItem, TKey>(item));

        BumpVersion(target);
        await Remove(target, ct);
        return Result.Ok();
    }

    protected void BumpVersion(TRecord target) => target.Version = generator.Next();

    protected abstract IQueryable<TRecord> GetData(Expression<Func<TRecord, bool>> expression);
    protected abstract Task<TRecord?> GetTarget(TItem item, CancellationToken ct);
    protected abstract Task<Result<TRecord>> Update(TRecord target, TItem updated, CancellationToken ct);
    protected abstract Task Remove(TRecord target, CancellationToken ct);
}
