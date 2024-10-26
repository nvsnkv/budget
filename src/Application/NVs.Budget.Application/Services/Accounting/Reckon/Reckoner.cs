﻿using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Services.Accounting.Reckon;

/// <summary>
/// Reads transactions etc
/// </summary>
internal class Reckoner(
    IStreamingOperationRepository operationsRepo,
    ITransfersRepository transfersRepo,
    MoneyConverter converter,
    DuplicatesDetector detector,
    IBudgetManager manager) : ReckonerBase(manager), IReckoner
{
    public async IAsyncEnumerable<TrackedOperation> GetOperations(OperationQuery query, [EnumeratorCancellation] CancellationToken ct)
    {
        var criteria = await ExtendCriteria(query.Conditions, ct);

        var transactions = operationsRepo.Get(criteria, ct);

        IReadOnlyCollection<TrackedTransfer> transfers = [];
        if (query.ExcludeTransfers)
        {
            // I'm too lazy to write async-friendly algo here right now
            // TODO: avoid materialization here (load all transfers maybe?)
            var transactionsList = await transactions.ToListAsync(ct);
            var budgets = await Manager.GetOwnedBudgets(ct);
            var ids =  transactionsList.Select(t => t.Id).ToList();
            transfers = await transfersRepo.Get(t => ids.Contains(t.Source.Id) || ids.Contains(t.Sink.Id), ct);
            transfers = transfers
                .Where(t => budgets.Contains(t.Source.Budget) && budgets.Contains(t.Sink.Budget))
                .ToList();

            transactions = transactionsList.ToAsyncEnumerable();
        }

        var exclusions = transfers.SelectMany(t => t).Select(t => t.Id).ToHashSet();

        await foreach (var transaction in transactions.Where(t => !exclusions.Contains(t.Id)).WithCancellation(ct))
        {
            if (query.OutputCurrency is not null && query.OutputCurrency != transaction.Amount.GetCurrency())
            {
                yield return AsTrackedOperation(await converter.Convert(transaction, query.OutputCurrency!, ct));
            }
            else
            {
                yield return transaction;
            }
        }

        foreach (var transfer in transfers.Where(t => !t.Fee.IsZero()))
        {
            yield return AsTrackedOperation(transfer.AsTransaction());
        }
    }

    public async Task<CriteriaBasedLogbook> GetLogbook(LogbookQuery query, CancellationToken ct)
    {
        var logbook = new CriteriaBasedLogbook(query.LogbookCriterion);
        await foreach (var operation in GetOperations(query, ct))
        {
            logbook.Register(operation);
        }

        return logbook;
    }

    public async Task<IReadOnlyCollection<IReadOnlyCollection<TrackedOperation>>> GetDuplicates(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct)
    {
        criteria = await ExtendCriteria(criteria, ct);
        var operations = operationsRepo.Get(criteria, ct);
        return await detector.DetectDuplicates(operations, ct);
    }

    private TrackedOperation AsTrackedOperation(Operation operation) => new(operation.Id, operation.Timestamp, operation.Amount, operation.Description, AsTrackedAccount(operation.Budget), operation.Tags, operation.Attributes.AsReadOnly());

    private TrackedBudget AsTrackedAccount(Domain.Entities.Accounts.Budget budget) => budget is TrackedBudget ta ? ta : new TrackedBudget(budget.Id, budget.Name, budget.Owners, [], [], LogbookCriteria.Universal);

}
