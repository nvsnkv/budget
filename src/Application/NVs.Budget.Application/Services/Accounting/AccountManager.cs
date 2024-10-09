using FluentResults;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Services.Accounting;

internal class AccountManager(IAccountsRepository repository, IUser currentUser) : IAccountManager
{
    private readonly Owner _currentOwner = currentUser.AsOwner();

    public Task<IReadOnlyCollection<TrackedBudget>> GetOwnedAccounts(CancellationToken ct)
    {
        var id = _currentOwner.Id;
        return repository.Get(a => a.Owners.Any(o => o.Id == id), ct);
    }

    public async Task<Result<TrackedBudget>> Register(UnregisteredAccount newAccount, CancellationToken ct)
    {
        var existing = await repository.Get(
            a => a.Owners.Any(o => o.Id == _currentOwner.Id) && a.Name == newAccount.Name,
            ct);
        if (existing.Count != 0) return Result.Fail<TrackedBudget>(new AccountAlreadyExistsError());

        return await repository.Register(newAccount, _currentOwner, ct);
    }

    public async Task<Result> ChangeOwners(TrackedBudget budget, IReadOnlyCollection<Owner> owners, CancellationToken ct)
    {
        if (!budget.Owners.Contains(_currentOwner)) return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError());

        if (!owners.Contains(_currentOwner)) return Result.Fail(new CurrentOwnerLosesAccessToAccountError());

        var found = (await repository.Get(a => a.Id == budget.Id, ct)).FirstOrDefault();
        if (found is null) return Result.Fail(new AccountDoesNotExistError(budget.Id));

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

    public async Task<Result> Update(TrackedBudget budget, CancellationToken ct)
    {
        var found = (await repository.Get(a => a.Id == budget.Id, ct)).FirstOrDefault();
        if (found is null) return Result.Fail(new AccountDoesNotExistError(budget.Id));
        if (!found.Owners.Contains(_currentOwner)) return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError());

        found.Rename(budget.Name);
        var result = await repository.Update(found, ct);
        return result.ToResult();
    }

    public async Task<Result> Remove(TrackedBudget budget, CancellationToken ct)
    {
        if (!budget.Owners.Contains(_currentOwner)) return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError());
        if (budget.Owners.Count > 1) return Result.Fail(new AccountBelongsToMultipleOwnersError());

        await repository.Remove(budget, ct);

        return Result.Ok();
    }
}
