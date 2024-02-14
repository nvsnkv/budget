using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Services.Accounting.Errors;
using NVs.Budget.Application.Services.Accounting.Successes;
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
    private readonly AccountManager _manager = manager;

    public async Task<ImportResult> Register(IAsyncEnumerable<UnregisteredTransaction> transactions, ImportOptions options, CancellationToken ct)
    {
        importResultBuilder.Clear();
        transfersListBuilder.Clear();

        var accounts = (await _manager.GetOwnedAccounts(ct)).ToList();

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
            var saveTransferResults = new List<IReason>();
            foreach (var transfer in result.Transfers.Where(t => t.Accuracy >= options.TransferConfidenceLevel))
            {
                var saveTransferResult = await transfersRepository.Track(transfer, ct);
                if (saveTransferResult.IsSuccess)
                {
                    saveTransferResults.Add(new TransferTracked(transfer));
                }
                else
                {
                    saveTransferResults.AddRange(saveTransferResult.Reasons.Concat(saveTransferResults));
                }
            }
        }

        return new ImportResult(result.Transactions, result.Transfers, result.Duplicates, result.Reasons);
    }

    public async Task<Result> Update(IAsyncEnumerable<TrackedTransaction> transactions, CancellationToken ct)
    {
        var accounts = await _manager.GetOwnedAccounts(ct);
        var result = new Result();
        await foreach (var transaction in transactions.WithCancellation(ct))
        {
            if (accounts.All(a => transaction.Account != a))
            {
                result.Reasons.Add(new AccountDoesNotBelongToCurrentOwnerError()
                    .WithTransactionId(transaction)
                    .WithAccountId(transaction.Account));
                continue;
            }

            var updateResult = await transactionsRepository.Update(transaction, ct);
            if (updateResult.IsSuccess)
            {
                result.Reasons.Add(new TransactionUpdated(updateResult.Value));
            }
            else
            {
                result.Reasons.AddRange(updateResult.Reasons);
            }
        }

        return result;
    }

    public async Task<Result> Delete(Expression<Func<TrackedTransaction, bool>> criteria, CancellationToken ct)
    {
        criteria = await ExtendCriteria(criteria, ct);
        return await transactionsRepository.Delete(criteria, ct);
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
        var result = await _manager.Register(account, ct);
        if (!result.IsSuccess)
        {
            return Result.Fail(result.Errors);
        }

        registeredAccount = result.Value;
        accounts.Add(registeredAccount);

        return Result.Ok(registeredAccount);
    }
}
