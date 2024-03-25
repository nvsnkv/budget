using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class AccountsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator):
    RepositoryBase<TrackedAccount, Guid, StoredAccount>(mapper, versionGenerator), IAccountsRepository
{
    private readonly DbSet<StoredOwner> _owners = context.Owners;

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

        return Mapper.Map<TrackedAccount>(entry.Entity);
    }

    protected override IQueryable<StoredAccount> GetData(Expression<Func<StoredAccount, bool>> expression)
    {
        return context.Accounts.Include(a => a.Owners.Where(o => !o.Deleted)).Where(expression);
    }

    protected override Task<StoredAccount?> GetTarget(TrackedAccount item, CancellationToken ct)
    {
        return context.Accounts
            .Include(a => a.Owners.Where(o => !o.Deleted))
            .Where(a => a.Id == item.Id)
            .FirstOrDefaultAsync(ct);
    }

    protected override async Task<Result<StoredAccount>> Update(StoredAccount target, TrackedAccount updated, CancellationToken ct)
    {
        var ids = updated.Owners.Select(o => o.Id).ToArray();
        var newOwners = await context.Owners.Where(o => ids.Contains(o.Id) && !o.Deleted).ToListAsync(ct);
        var missedOwners = updated.Owners.Where(o => newOwners.All(so => so.Id != o.Id)).ToList();
        if (missedOwners.Count != 0)
        {
            var reasons = missedOwners.Select(o => new EntityDoesNotExistError<Owner>(o));
            return Result.Fail(reasons);
        }

        target.Name = updated.Name;
        target.Bank = updated.Bank;
        foreach (var toRemove in target.Owners.Except(newOwners))
        {
            target.Owners.Remove(toRemove);
        }

        foreach (var toAdd in newOwners.Except(target.Owners))
        {
            target.Owners.Add(toAdd);
        }

        await context.SaveChangesAsync(ct);
        return Result.Ok(target);
    }

    protected override Task Remove(StoredAccount target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }
}
