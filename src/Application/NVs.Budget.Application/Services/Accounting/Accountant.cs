using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Results;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Application.Services.Accounting.Results.Successes;
using NVs.Budget.Application.Services.Accounting.Tags;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Extensions;

namespace NVs.Budget.Application.Services.Accounting;

/// <summary>
/// Manages transactions (register, update, delete etc)
/// </summary>
internal class Accountant(
    IOperationsRepository operationsRepository,
    ITransfersRepository transfersRepository,
    IAccountManager manager,
    TagsManager tagsManager,
    TransfersListBuilder transfersListBuilder,
    ImportResultBuilder importResultBuilder) :ReckonerBase(manager), IAccountant
{
    public async Task<ImportResult> ImportTransactions(IAsyncEnumerable<UnregisteredOperation> transactions, ImportOptions options, CancellationToken ct)
    {
        importResultBuilder.Clear();
        transfersListBuilder.Clear();

        var accounts = (await Manager.GetOwnedAccounts(ct)).ToList();

        await foreach (var unregisteredTransaction in transactions.WithCancellation(ct))
        {
            var accountResult = await TryGetAccount(accounts, unregisteredTransaction.Account, options.RegisterAccounts, ct);
            importResultBuilder.Append(accountResult);
            if (!accountResult.IsSuccess)
            {
                continue;
            }

            var transactionResult = await operationsRepository.Register(unregisteredTransaction, accountResult.Value, ct);
            importResultBuilder.Append(transactionResult);

            if (transactionResult.IsSuccess)
            {
                var transaction = transactionResult.Value;
                foreach (var tag in tagsManager.GetTags(transaction))
                {
                    transaction.Tag(tag);
                }

                if (transaction.Tags.Count > 0)
                {
                    transactionResult = await operationsRepository.Update(transaction, ct);
                    if (!transactionResult.IsSuccess)
                    {
                        importResultBuilder.Append(transactionResult);
                    }
                    else
                    {
                        transaction = transactionResult.Value;
                    }
                }

                transfersListBuilder.Add(transaction);
            }
        }

        foreach (var transfer in transfersListBuilder.ToList())
        {
            importResultBuilder.Append(transfer);
        }

        var result = importResultBuilder.Build();

        if (options.TransferConfidenceLevel is not null)
        {
            var transfers = result.Transfers
                .Where(t => t.Accuracy >= options.TransferConfidenceLevel)
                .Select(t => new UnregisteredTransfer(
                    t.Source as TrackedOperation ?? throw new InvalidOperationException("Attempt was made to register transfer with unregistered transaction!").WithOperationId(t.Source),
                    t.Sink as TrackedOperation ?? throw new InvalidOperationException("Attempt was made to register transfer with unregistered transaction!").WithOperationId(t.Sink),
                    t.Fee,
                    t.Comment,
                    t.Accuracy))
                .ToAsyncEnumerable();

            var registrationResult = await RegisterTransfers(transfers, ct);
            result.Reasons.AddRange(registrationResult.Reasons);
        }

        return new ImportResult(result.Operations, result.Transfers, result.Duplicates, result.Reasons);
    }

    public async Task<Result> Update(IAsyncEnumerable<TrackedOperation> transactions, CancellationToken ct)
    {
        var accounts = await Manager.GetOwnedAccounts(ct);
        var result = new Result();
        await foreach (var transaction in transactions.WithCancellation(ct))
        {
            var updateResult = await Update(transaction, accounts, ct);
            result.Reasons.AddRange(updateResult.Reasons);
        }

        return result;
    }

    public async Task<Result> Delete(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct)
    {
        criteria = await ExtendCriteria(criteria, ct);
        var targets = await operationsRepository.Get(criteria, ct);
        var successes = new List<Success>();
        var errors = new List<IError>();
        foreach (var target in targets)
        {
            var opResult = await operationsRepository.Remove(target, ct);
            if (opResult.IsSuccess)
            {
                successes.Add(new OperationRemoved(target));
            }
            else
            {
                errors.AddRange(opResult.Errors);
            }
        }

        var result = errors.Any() ? Result.Fail(errors) : Result.Ok();
        result.Reasons.AddRange(successes);

        return result;
    }

    public async Task<Result> RegisterTransfers(IAsyncEnumerable<UnregisteredTransfer> transfers, CancellationToken ct)
    {
        var accounts = await Manager.GetOwnedAccounts(ct);
        var result = new Result();

        await foreach (var transfer in transfers.WithCancellation(ct))
        {
            if (accounts.All(a => a != transfer.Source.Account))
            {
                result.Reasons.Add(new AccountDoesNotBelongToCurrentOwnerError().WithAccountId(transfer.Source.Account));
                continue;
            }

            if (accounts.All(a => a != transfer.Sink.Account))
            {
                result.Reasons.Add(new AccountDoesNotBelongToCurrentOwnerError().WithAccountId(transfer.Sink.Account));
                continue;
            }

            transfer.Source.TagSource();
            transfer.Sink.TagSink();

            var updateResult = await operationsRepository.Update(transfer.Source, ct);
            if (!updateResult.IsSuccess)
            {
                result.Reasons.Add(new UnableToTagTransferError(updateResult.Errors).WithTransactionId(transfer.Source));
                continue;
            }

            updateResult = await operationsRepository.Update(transfer.Sink, ct);
            if (!updateResult.IsSuccess)
            {
                result.Reasons.Add(new UnableToTagTransferError(updateResult.Errors).WithTransactionId(transfer.Sink));
                continue;
            }

            var trackedTransfer = new TrackedTransfer(transfer.Source, transfer.Sink, transfer.Fee, transfer.Comment) {Accuracy = transfer.Accuracy};
            var registrationResult = await transfersRepository.Register(trackedTransfer, ct);
            if (registrationResult.IsSuccess)
            {
                result.Reasons.Add(new TransferTracked(trackedTransfer));
            }
            else
            {
                result.Reasons.AddRange(registrationResult.Errors);
            }
        }

        return result;
    }

    private async Task<Result<TrackedOperation>> Update(TrackedOperation operation, IReadOnlyCollection<TrackedAccount> ownedAccounts, CancellationToken ct)
    {
        if (ownedAccounts.All(a => operation.Account != a))
        {
            return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError()
                .WithTransactionId(operation)
                .WithAccountId(operation.Account));
        }

        var updateResult = await operationsRepository.Update(operation, ct);
        return updateResult.IsSuccess
            ? Result.Ok(updateResult.Value).WithReason(new OperationUpdated(updateResult.Value))
            : Result.Fail(updateResult.Errors);
    }

    private async Task<Result<TrackedAccount>> TryGetAccount(ICollection<TrackedAccount> accounts, UnregisteredAccount account, bool shouldRegister, CancellationToken ct)
    {
        var registeredAccount = accounts.FirstOrDefault(a => a.Name == account.Name && a.Bank == account.Bank);
        if (registeredAccount is not null) return Result.Ok(registeredAccount);

        if (!shouldRegister)
        {
            return Result.Fail(new AccountNotFoundError()
                .WithMetadata(nameof(TrackedAccount.Name), account.Name)
                .WithMetadata(nameof(TrackedAccount.Bank), account.Bank)
            );
        }
        var result = await Manager.Register(account, ct);
        if (!result.IsSuccess)
        {
            return Result.Fail(result.Errors);
        }

        registeredAccount = result.Value;
        accounts.Add(registeredAccount);

        return Result.Ok(registeredAccount);
    }
}
