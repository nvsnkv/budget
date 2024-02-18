using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Application.Services.Storage.Accounting;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Transactions;

namespace NVs.Budget.Application.Services.Accounting.Reckon;

/// <summary>
/// Reads transactions etc
/// </summary>
internal class Reckoner(
    ITransactionsRepository transactionsRepo,
    ITransfersRepository transfersRepo,
    MoneyConverter converter,
    DuplicatesDetector detector,
    AccountManager manager) : ReckonerBase(manager)
{
    private static readonly TrackedTransfer[] Empty = [];

    public async IAsyncEnumerable<Transaction> GetTransactions(TransactionQuery query, [EnumeratorCancellation] CancellationToken ct)
    {
        var criteria = await ExtendCriteria(query.Conditions, ct);

        var transactions = await transactionsRepo.Get(criteria, ct);

        IReadOnlyCollection<TrackedTransfer> transfers = Empty;
        if (query.ExcludeTransfers)
        {
            var accounts = await Manager.GetOwnedAccounts(ct);
            var ids = transactions.Select(t => t.Id).ToList();
            transfers = await transfersRepo.Get(t => ids.Contains(t.Source.Id) || ids.Contains(t.Sink.Id), ct);
            transfers = transfers
                .Where(t => accounts.Contains(t.Source.Account) && accounts.Contains(t.Sink.Account))
                .ToList();
        }

        var exclusions = transfers.SelectMany(t => t).Select(t => t.Id).ToHashSet();

        foreach (var transaction in transactions.Where(t => !exclusions.Contains(t.Id)))
        {
            if (query.OutputCurrency is not null && query.OutputCurrency != transaction.Amount.GetCurrency())
            {
                yield return await converter.Convert(transaction, query.OutputCurrency!, ct);
            }
            else
            {
                yield return transaction;
            }
        }

        foreach (var transfer in transfers.Where(t => !t.Fee.IsZero()))
        {
            yield return transfer.AsTransaction();
        }
    }


    public async Task<CriteriaBasedLogbook> GetLogbook(LogbookQuery query, CancellationToken ct)
    {
        var logbook = new CriteriaBasedLogbook(query.LogbookCriterion);
        await foreach (var transaction in GetTransactions(query, ct))
        {
            logbook.Register(transaction);
        }

        return logbook;
    }

    public async Task<IReadOnlyCollection<IReadOnlyCollection<TrackedTransaction>>> GetDuplicates(Expression<Func<TrackedTransaction, bool>> criteria, CancellationToken ct)
    {
        criteria = await ExtendCriteria(criteria, ct);
        var transactions = await transactionsRepo.Get(criteria, ct);
        return detector.DetectDuplicates(transactions);
    }
}
