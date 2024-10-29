using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Queries;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Exchange;
using NVs.Budget.Application.Services.Accounting.Transfers;
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

        var sourceIds = new List<Guid>();
        var sinkIds = new List<Guid>();

        if (query.ExcludeTransfers)
        {
            transactions = transactions.Where(t =>
            {
                if (t.Tags.Contains(TransferTags.Source))
                {
                    sourceIds.Add(t.Id);
                    return false;
                }

                if (t.Tags.Contains(TransferTags.Sink))
                {
                    sinkIds.Add(t.Id);
                    return false;
                }

                return true;
            });
        }

        await foreach (var transaction in transactions.WithCancellation(ct))
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

        if (sourceIds.Count > 0 || sinkIds.Count > 0)
        {
            var budgets = await Manager.GetOwnedBudgets(ct);
            var ids = budgets.Select(b => b.Id).ToHashSet();

            var transfers = transfersRepo.Get(t => sourceIds.Contains(t.Source.Id) || sinkIds.Contains(t.Sink.Id), ct);
            await foreach (var transfer in transfers)
            {
                if (ids.Contains(transfer.Source.Budget.Id) && ids.Contains(transfer.Sink.Budget.Id))
                {
                    if (!transfer.Fee.IsZero())
                    {
                        yield return AsTrackedOperation(transfer.AsTransaction());
                    }
                }

                else if (ids.Contains(transfer.Source.Budget.Id))
                {
                    yield return transfer.Source as TrackedOperation ?? AsTrackedOperation(transfer.Source);
                }
                else if(ids.Contains(transfer.Sink.Budget.Id))
                {
                    yield return transfer.Sink as TrackedOperation ?? AsTrackedOperation(transfer.Sink);
                }
            }
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

    private TrackedOperation AsTrackedOperation(Operation operation)
    {
        var result = new TrackedOperation(
            operation.Id, operation.Timestamp, operation.Amount, operation.Description,
            AsTrackedAccount(operation.Budget), operation.Tags, operation.Attributes.AsReadOnly()
        );

        result.TagEphemeral();
        result.Version = null;
        return result;
    }

    private TrackedBudget AsTrackedAccount(Domain.Entities.Accounts.Budget budget) => budget is TrackedBudget ta ? ta : new TrackedBudget(budget.Id, budget.Name, budget.Owners, [], [], LogbookCriteria.Universal);

}
