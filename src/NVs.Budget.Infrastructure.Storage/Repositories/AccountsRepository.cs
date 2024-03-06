using System.Linq.Expressions;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;
using NVs.Budget.Infrastructure.Storage.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal class AccountsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator): IAccountsRepository
{
    private readonly DbSet<StoredOwner> _owners = context.Owners;

    public async Task<IReadOnlyCollection<TrackedAccount>> Get(Expression<Func<TrackedAccount, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedAccount, StoredAccount>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var items = await context.Accounts
            .Include(a => a.Owners)
            .AsNoTracking()
            .Where(expression)
            .ProjectTo<TrackedAccount>(mapper.ConfigurationProvider)
            .ToListAsync(ct);

        return items;
    }

    public async Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, Owner owner, CancellationToken ct)
    {
        var storedOwner = await _owners.FirstOrDefaultAsync(o => o.Id == owner.Id, ct);
        if (storedOwner is null)
        {
            return Result.Fail(new OwnerIsNotRegisteredError());
        }

        var account = new StoredAccount(Guid.Empty, newAccount.Name, newAccount.Bank)
        {
            Owners = { storedOwner }
        };

        BumpVersion(account);

        var entry = await context.Accounts.AddAsync(account, ct);
        await context.SaveChangesAsync(ct);

        return mapper.Map<TrackedAccount>(entry.Entity);
    }

    public async Task<Result<TrackedAccount>> Update(TrackedAccount account, CancellationToken ct)
    {
        var result = await GetTarget(account, true, ct);
        if (result.IsFailed)
        {
            return result.ToResult<TrackedAccount>();
        }

        var target = result.Value;

        var ids = account.Owners.Select(o => o.Id).ToArray();
        var newOwners = await context.Owners.Where(o => ids.Contains(o.Id) && !o.Deleted).ToListAsync(ct);
        var missedOwners = account.Owners.Where(o => newOwners.All(so => so.Id != o.Id)).ToList();
        if (missedOwners.Count != 0)
        {
            var reasons = missedOwners.Select(o => new EntityDoesNotExistError<Owner>(o));
            return Result.Fail(reasons);
        }

        target.Name = account.Name;
        target.Bank = account.Bank;
        foreach (var toRemove in target.Owners.Except(newOwners))
        {
            target.Owners.Remove(toRemove);
        }

        foreach (var toAdd in newOwners.Except(target.Owners))
        {
            target.Owners.Add(toAdd);
        }

        BumpVersion(target);
        await context.SaveChangesAsync(ct);
        return mapper.Map<TrackedAccount>(target);
    }

    public async Task<Result> Remove(TrackedAccount account, CancellationToken ct)
    {
        var result = await GetTarget(account, false, ct);
        if (result.IsFailed)
        {
            return result.ToResult();
        }

        var target = result.Value;
        target.Deleted = true;
        BumpVersion(target);

        await context.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private void BumpVersion(StoredAccount account)
    {
        account.Version = versionGenerator.Next();
    }

    private async Task<Result<StoredAccount>> GetTarget(TrackedAccount entity, bool includeOwners, CancellationToken ct)
    {
        IQueryable<StoredAccount> query = context.Accounts;
        if (includeOwners)
        {
            query = query.Include(a => a.Owners.Where(o => !o.Deleted));
        }
        var target = await query.FirstOrDefaultAsync(a => a.Id == entity.Id, ct);
        if (target is null)
        {
            return Result.Fail(new EntityDoesNotExistError<TrackedAccount>(entity));
        }

        return target.Version != entity.Version
            ? Result.Fail(new VersionDoesNotMatchError<TrackedAccount, Guid>(entity))
            : Result.Ok(target);
    }
}
