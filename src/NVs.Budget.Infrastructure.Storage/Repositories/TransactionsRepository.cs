using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;
using NVs.Budget.Infrastructure.Storage.Repositories.Results;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal class TransactionsRepository(IMapper mapper, BudgetContext context, VersionGenerator versionGenerator) :
    RepositoryBase<TrackedTransaction, Guid, StoredTransaction>(mapper, versionGenerator), ITransactionsRepository
{
    protected override IQueryable<StoredTransaction> GetData(Expression<Func<StoredTransaction, bool>> expression) =>
        context.Transactions
            .Include(t => t.Account)
            .ThenInclude(a => a.Owners.Where(o => !o.Deleted))
            .Where(expression);

    protected override Task<StoredTransaction?> GetTarget(TrackedTransaction item, CancellationToken ct)
    {
        var id = item.Id;
        return context.Transactions.FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    protected override async Task<Result<StoredTransaction>> Update(StoredTransaction target, TrackedTransaction updated, CancellationToken ct)
    {
        if (target.Account.Id != updated.Account.Id)
        {
            return Result.Fail(new CannotChangeAccountError(updated));
        }
        else
        {
            target.Amount = Mapper.Map<StoredMoney>(updated.Amount);
            target.Timestamp = updated.Timestamp;
            target.Description = updated.Description;

            UpdateTags(target.Tags, updated.Tags);
            target.Attributes = updated.Attributes.ToJson();

            await context.SaveChangesAsync(ct);

            return Result.Ok(target);
        }
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

    protected override Task Remove(StoredTransaction target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }

    public async Task<Result<TrackedTransaction>> Register(UnregisteredTransaction transaction, TrackedAccount account, CancellationToken ct)
    {
        var storedAccount = await context.Accounts.Include(a => a.Owners.Where(o => !o.Deleted))
            .FirstOrDefaultAsync(a => a.Id == account.Id, ct);
        if (storedAccount is null)
        {
            return Result.Fail(new AccountDoesNotExistsError(account));
        }

        var storedTransaction = new StoredTransaction(Guid.Empty, transaction.Timestamp, transaction.Description)
        {
            Account = storedAccount,
            Amount = Mapper.Map<StoredMoney>(transaction.Amount),
            Attributes = new Dictionary<string, object>(transaction.Attributes ?? Enumerable.Empty<KeyValuePair<string, object>>()).ToJson()
        };

        BumpVersion(storedTransaction);
        await context.Transactions.AddAsync(storedTransaction, ct);
        await context.SaveChangesAsync(ct);

        return Mapper.Map<TrackedTransaction>(storedTransaction);
    }
}
