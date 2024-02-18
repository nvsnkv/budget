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
    ITransactionsRepository transactionsRepository,
    AccountManager manager,
    TransfersListBuilder transfersListBuilder,
    ITransfersRepository transfersRepository,
    ImportResultBuilder importResultBuilder,
    TagsManager tagsManager) :ReckonerBase(manager)
{
    public async Task<ImportResult> ImportTransactions(IAsyncEnumerable<UnregisteredTransaction> transactions, ImportOptions options, CancellationToken ct)
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

            var transactionResult = await transactionsRepository.Register(unregisteredTransaction, accountResult.Value, ct);
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
                    transactionResult = await transactionsRepository.Update(transaction, ct);
                    if (!transactionResult.IsSuccess)
                    {
                        importResultBuilder.Append(transactionResult);
                    }
                }
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
                    t.Source as TrackedTransaction ?? throw new InvalidOperationException("Attempt was made to register transfer with unregistered transaction!").WithTransactionId(t.Source),
                    t.Sink as TrackedTransaction ?? throw new InvalidOperationException("Attempt was made to register transfer with unregistered transaction!").WithTransactionId(t.Sink),
                    t.Fee,
                    t.Comment,
                    t.Accuracy))
                .ToAsyncEnumerable();

            var registrationResult = await RegisterTransfers(transfers, ct);
            result.Reasons.AddRange(registrationResult.Reasons);
        }

        return new ImportResult(result.Transactions, result.Transfers, result.Duplicates, result.Reasons);
    }

    public async Task<Result> Update(IAsyncEnumerable<TrackedTransaction> transactions, CancellationToken ct)
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

    public async Task<Result> Delete(Expression<Func<TrackedTransaction, bool>> criteria, CancellationToken ct)
    {
        criteria = await ExtendCriteria(criteria, ct);
        return await transactionsRepository.Remove(criteria, ct);
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

            var updateResult = await transactionsRepository.Update(transfer.Source, ct);
            if (!updateResult.IsSuccess)
            {
                result.Reasons.Add(new UnableToTagTransferError(updateResult.Errors).WithTransactionId(transfer.Source));
                continue;
            }

            updateResult = await transactionsRepository.Update(transfer.Sink, ct);
            if (!updateResult.IsSuccess)
            {
                result.Reasons.Add(new UnableToTagTransferError(updateResult.Errors).WithTransactionId(transfer.Sink));
                continue;
            }

            var trackedTransfer = new TrackedTransfer(transfer.Source, transfer.Sink, transfer.Fee, transfer.Comment) {Accuracy = transfer.Accuracy};
            var registrationResult = await transfersRepository.Register(trackedTransfer, ct);
            result.Reasons.AddRange(registrationResult.Reasons);
        }

        return result;
    }

    private async Task<Result<TrackedTransaction>> Update(TrackedTransaction transaction, IReadOnlyCollection<TrackedAccount> ownedAccounts, CancellationToken ct)
    {
        if (ownedAccounts.All(a => transaction.Account != a))
        {
            return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError()
                .WithTransactionId(transaction)
                .WithAccountId(transaction.Account));
        }

        var updateResult = await transactionsRepository.Update(transaction, ct);
        return updateResult.IsSuccess
            ? Result.Ok(updateResult.Value).WithReason(new TransactionUpdated(updateResult.Value))
            : Result.Fail(updateResult.Errors);
    }

    private async Task<Result<TrackedAccount>> TryGetAccount(List<TrackedAccount> accounts, UnregisteredAccount account, bool shouldRegister, CancellationToken ct)
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
