using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Transactions;
using FluentResults;
using NVs.Budget.Application.Entities.Contracts;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Utilities.Expressions;
using Transaction = NVs.Budget.Domain.Entities.Transactions.Transaction;

namespace NVs.Budget.Application.Services.Accounting;

internal class Accountant(
    ITransactionsRepository repository,
    AccountManager manager,
    TransferDetector transferDetector,
    DuplicatesDetector duplicatesDetector,
    TagsManager tagsManager)
{
    public async Task<IReadOnlyCollection<TrackedTransaction>> GetTransactions(Expression<Func<TrackedTransaction, bool>> filter, CancellationToken ct)
    {
        var accounts = await manager.GetOwnedAccounts(ct);
        filter = filter.CombineWith(t => accounts.Contains(t.Account));

        return await repository.GetTransactions(filter, ct);
    }

    public async Task<ImportResult> Register(IAsyncEnumerable<UnregisteredTransaction> transactions, CancellationToken ct)
    {
        var result = new ImportResult();
        var accounts = (await manager.GetOwnedAccounts(ct)).ToList();
        var sources = new List<TrackedTransaction>();
        var sinks = new List<TrackedTransaction>();

        await foreach (var unregisteredTransaction in transactions.WithCancellation(ct))
        {
            var accountResult = await TryGetAccount(accounts, unregisteredTransaction.Account, ct);
            if (!accountResult.IsSuccess)
            {
                result.Append(accountResult.ToResult<TrackedTransaction>());
            }

            var transactionResult = await repository.Register(unregisteredTransaction, accountResult.Value, ct);
            result.Append(transactionResult);

            if (transactionResult.IsSuccess)
            {
                var transaction = transactionResult.Value;
                foreach (var tag in tagsManager.GetTags(transaction))
                {
                    transaction.Tag(tag);
                }

                if (transaction.Tags.Count > 0)
                {
                    transactionResult = await repository.Update(transaction, ct);
                    if (!transactionResult.IsSuccess)
                    {
                        result.Append(transactionResult);
                    }
                    else
                    {
                        transaction = transactionResult.Value;
                    }
                }

                var transferDetected = false;
                if (transaction.Amount.Amount < 0)
                {
                    foreach (var sink in sinks)
                    {
                        var detectionResult = await transferDetector.Detect(transaction, sink, ct);
                        if (detectionResult.IsSuccess)
                        {
                            result.Append(detectionResult.Value);
                            sinks.Remove(sink);
                            transferDetected = true;
                            break;
                        }
                    }

                    if (!transferDetected)
                    {
                        sources.Add(transaction);
                    }
                }
                else
                {
                    foreach (var source in sources)
                    {
                        var detectionResult = await transferDetector.Detect(source, transaction, ct);
                        if (detectionResult.IsSuccess)
                        {
                            result.Append(detectionResult.Value);
                            sources.Remove(source);
                            transferDetected = true;
                            break;
                        }
                    }

                    if (!transferDetected)
                    {
                        sinks.Add(transaction);
                    }
                }
            }
        }

        result.Duplicates = duplicatesDetector.FindDuplicates(result.Transactions);

        return result;
    }

    private async Task<Result<TrackedAccount>> TryGetAccount(List<TrackedAccount> accounts, UnregisteredAccount account, CancellationToken ct)
    {
        var registeredAccount = accounts.FirstOrDefault(a => a.Name == account.Name && a.Bank == account.Bank);
        if (registeredAccount is not null) return Result.Ok(registeredAccount);

        var result = await manager.Register(account, ct);
        if (!result.IsSuccess)
        {
            return Result.Fail(result.Errors);
        }

        registeredAccount = result.Value;
        accounts.Add(registeredAccount);

        return Result.Ok(registeredAccount);
    }
}

public class ImportResult : Result
{
    private readonly List<TrackedTransaction> _transactions = new();
    private readonly List<TrackedTransfer> _transfers = new();

    public void Append(Result<TrackedTransaction> result)
    {
        if (result.IsSuccess)
        {
            Reasons.Add(new Success("Added transaction").WithTransactionId(result.Value));
            _transactions.Add(result.Value);
        }
        else
        {
            Reasons.AddRange(result.Errors);
        }
    }

    public IReadOnlyCollection<TrackedTransaction> Transactions => _transactions.AsReadOnly();

    public IReadOnlyCollection<TrackedTransfer> Transfers => _transfers.AsReadOnly();

    public void Append(TrackedTransfer transfer)
    {
        _transfers.Add(transfer);
    }

    public IReadOnlyCollection<IReadOnlyCollection<TrackedTransaction>>? Duplicates { get; set; }
}
