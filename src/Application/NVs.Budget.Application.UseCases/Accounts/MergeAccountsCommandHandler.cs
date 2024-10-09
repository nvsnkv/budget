using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Contracts.UseCases.Accounts;

namespace NVs.Budget.Application.UseCases.Accounts;

internal class MergeAccountsCommandHandler(IReckoner reckoner, IAccountant accountant, IAccountManager manager) : IRequestHandler<MergeAccountsCommand, Result>
{
    public async Task<Result> Handle(MergeAccountsCommand request, CancellationToken cancellationToken)
    {
        var accounts = (await manager.GetOwnedAccounts(cancellationToken)).ToDictionary(a => a.Id);
        var source = accounts.GetValueOrDefault(request.SourceId);
        if (source is null) return Result.Fail(new AccountNotFoundError(request.SourceId));

        var target = accounts.GetValueOrDefault(request.TargetId);
        if (target is null) return Result.Fail(new AccountNotFoundError(request.TargetId));

        var operations = reckoner.GetOperations(new(o => o.Budget.Id == source.Id), cancellationToken)
            .Select(o => new TrackedOperation(o.Id, o.Timestamp, o.Amount, o.Description, target, o.Tags, o.Attributes.AsReadOnly()));

        var result = await accountant.Update(operations, cancellationToken);
        if (!result.IsFailed)
        {
            var accResult = await manager.Remove(source, cancellationToken);
            result.Reasons.AddRange(accResult.Reasons);
        }

        return result;
    }
}
