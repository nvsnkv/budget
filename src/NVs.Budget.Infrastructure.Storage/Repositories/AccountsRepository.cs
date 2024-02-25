using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;
using NVs.Budget.Infrastructure.Storage.Repositories.Results;

namespace NVs.Budget.Infrastructure.Storage.Repositories;

internal class AccountsRepository(IMapper mapper, BudgetContext context)
    : AbstractRepository<TrackedAccount, StoredAccount>(mapper, context, c => c.Accounts),
      IAccountsRepository
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

        return await DoInsert(account, ct);
    }
}
