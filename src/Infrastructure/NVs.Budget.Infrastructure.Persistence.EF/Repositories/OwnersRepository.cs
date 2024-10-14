using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class OwnersRepository(IMapper mapper, BudgetContext context, VersionGenerator generator) : RepositoryBase<TrackedOwner, Guid, StoredOwner>(mapper, generator), IOwnersRepository
{
    protected override IQueryable<StoredOwner> GetData(Expression<Func<StoredOwner, bool>> expression) => context.Owners.Where(expression);

    protected override Task<StoredOwner?> GetTarget(TrackedOwner item, CancellationToken ct) => context.Owners.FirstOrDefaultAsync(o => o.Id == item.Id, ct);

    protected override Task<Result<StoredOwner>> Update(StoredOwner target, TrackedOwner updated, CancellationToken ct)
    {
        target.Name = updated.Name;
        return Task.FromResult(Result.Ok(target));
    }

    protected override Task Remove(StoredOwner target, CancellationToken ct)
    {
        target.Deleted = true;
        return context.SaveChangesAsync(ct);
    }

    public async Task<Result<TrackedOwner>> Register(IUser user, CancellationToken ct)
    {
        var newOwner = user.AsOwner();
        var existing = await context.Owners.FirstOrDefaultAsync(u => u.UserId == user.Id, ct);
        if (existing is not null)
        {
            return Result.Fail(new OwnerIsAlreadyRegisteredError().WithMetadata(nameof(Owner.Id), existing.Id));
        }

        var storedOwner = new StoredOwner(Guid.Empty, newOwner.Name)
        {
            UserId = user.Id
        };

        await context.Owners.AddAsync(storedOwner, ct);
        await context.SaveChangesAsync(ct);

        return Mapper.Map<TrackedOwner>(storedOwner);
    }

    public async Task<TrackedOwner?> Get(IUser user, CancellationToken ct)
    {
        var owner = await context.Owners.FirstOrDefaultAsync(o => o.UserId == user.Id, ct);
        return Mapper.Map<TrackedOwner?>(owner);
    }
}
