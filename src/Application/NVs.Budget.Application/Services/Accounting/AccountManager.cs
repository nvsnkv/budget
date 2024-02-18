using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Entities.Contracts;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Services.Accounting;

internal class AccountManager(IAccountsRepository repository, IUser currentUser) : IAccountManager
{
    private readonly Owner _currentOwner = currentUser.AsOwner();

    public Task<IReadOnlyCollection<TrackedAccount>> GetOwnedAccounts(CancellationToken ct)
    {
        return repository.Get(a => a.Owners.Contains(_currentOwner), ct);
    }

    public async Task<Result<TrackedAccount>> Register(UnregisteredAccount newAccount, CancellationToken ct)
    {
        var existing = await repository.Get(
            a => a.Owners.Contains(_currentOwner) && a.Name == newAccount.Name && a.Bank == newAccount.Bank,
            ct);
        if (existing.Count != 0) return Result.Fail<TrackedAccount>(new AccountAlreadyExistsError());

        return await repository.Register(newAccount, _currentOwner, ct);
    }

    public async Task<Result> ChangeOwners(TrackedAccount account, IReadOnlyCollection<Owner> owners, CancellationToken ct)
    {
        if (!account.Owners.Contains(_currentOwner)) return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError());

        if (!owners.Contains(_currentOwner)) return Result.Fail(new CurrentOwnerLosesAccessToAccountError());

        var found = (await repository.Get(a => a.Id == account.Id, ct)).FirstOrDefault();
        if (found is null) return Result.Fail(new AccountDoesNotExistError(account.Id));

        foreach (var exOwner in found.Owners.Except(owners).ToList())
        {
            found.RemoveOwner(exOwner);
        }

        foreach (var owner in owners.Except(found.Owners).ToList())
        {
            found.AddOwner(owner);
        }

        var result = await repository.Update(found, ct);
        return result.ToResult();
    }

    public async Task<Result> Update(TrackedAccount account, CancellationToken ct)
    {
        var found = (await repository.Get(a => a.Id == account.Id, ct)).FirstOrDefault();
        if (found is null) return Result.Fail(new AccountDoesNotExistError(account.Id));
        if (!found.Owners.Contains(_currentOwner)) return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError());

        found.Rename(account.Name, account.Bank);
        var result = await repository.Update(found, ct);
        return result.ToResult();
    }

    public async Task<Result> Remove(TrackedAccount account, CancellationToken ct)
    {
        if (!account.Owners.Contains(_currentOwner)) return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError());
        if (account.Owners.Count > 1) return Result.Fail(new AccountBelongsToMultipleOwnersError());

        await repository.Remove(account, ct);

        return Result.Ok();
    }
}
