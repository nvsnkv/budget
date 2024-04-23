using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.ExpressionVisitors;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class OperationsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator, AccountsFinder finder) :
    RepositoryBase<TrackedOperation, Guid, StoredOperation>(mapper, versionGenerator), IOperationsRepository
{
    private readonly ExpressionSplitter _splitter = new();

    public override async Task<IReadOnlyCollection<TrackedOperation>> Get(Expression<Func<TrackedOperation, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedOperation, StoredOperation>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var (queryable, enumerable) = _splitter.Split(expression);
        var query = context.Operations
            .Include(t => t.Account)
            .ThenInclude(a => a.Owners.Where(o => !o.Deleted))
            .Where(queryable);

        var items = await query.AsNoTracking().ToListAsync(ct);
        items = items.Where(enumerable).ToList();
        return Mapper.Map<List<TrackedOperation>>(items).AsReadOnly();
    }

    protected override Task<StoredOperation?> GetTarget(TrackedOperation item, CancellationToken ct)
    {
        var id = item.Id;
        return context.Operations.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    protected override async Task<Result<StoredOperation>> Update(StoredOperation target, TrackedOperation updated, CancellationToken ct)
    {
        if (target.Account.Id != updated.Account.Id)
        {
            return Result.Fail(new CannotChangeAccountError(updated));
        }

        target.Amount = Mapper.Map<StoredMoney>(updated.Amount);
        target.Timestamp = updated.Timestamp;
        target.Description = updated.Description;

        UpdateTags(target.Tags, updated.Tags);
        target.Attributes = updated.Attributes.ToDictionary();

        await context.SaveChangesAsync(ct);

        return Result.Ok(target);
    }

    private void UpdateTags(IList<StoredTag> targetTags, IReadOnlyCollection<Tag> updatedTags)
    {
        var updated = updatedTags.Select(t => Mapper.Map<StoredTag>(t)).ToList();
        var toRemove = targetTags.Except(updated).ToList();
        var toAdd = updated.Except(targetTags).ToList();

        foreach (var tag in toRemove)
        {
            targetTags.Remove(tag);
        }

        foreach (var tag in toAdd)
        {
            targetTags.Add(tag);
        }
    }

    protected override Task Remove(StoredOperation target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }

    public async Task<Result<TrackedOperation>> Register(UnregisteredOperation operation, TrackedAccount account, CancellationToken ct)
    {
        var storedAccount = await finder.FindById(account.Id, ct);
        if (storedAccount is null)
        {
            return Result.Fail(new AccountDoesNotExistsError(account));
        }

        var storedTransaction = new StoredOperation(Guid.Empty, operation.Timestamp.ToUniversalTime(), operation.Description)
        {
            Account = storedAccount,
            Amount = Mapper.Map<StoredMoney>(operation.Amount),
            Attributes = new Dictionary<string, object>(operation.Attributes ?? Enumerable.Empty<KeyValuePair<string, object>>())
        };

        BumpVersion(storedTransaction);
        await context.Operations.AddAsync(storedTransaction, ct);
        await context.SaveChangesAsync(ct);

        return Mapper.Map<TrackedOperation>(storedTransaction);
    }

    protected override IQueryable<StoredOperation> GetData(Expression<Func<StoredOperation, bool>> expression)
    {
        throw new NotImplementedException();
    }
}
