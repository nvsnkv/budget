using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Errors.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Results;
using NVs.Budget.Application.Services.Accounting.Results.Errors;
using NVs.Budget.Application.Services.Accounting.Results.Successes;
using NVs.Budget.Application.Services.Accounting.Tags;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.Services.Accounting;

/// <summary>
/// Manages transactions (register, update, delete etc)
/// </summary>
internal class Accountant(
    IStreamingOperationRepository streamingOperationRepository,
    ITransfersRepository transfersRepository,
    IBudgetManager manager,
    DuplicatesDetector detector) :ReckonerBase(manager), IAccountant
{
    public async Task<ImportResult> ImportOperations(IAsyncEnumerable<UnregisteredOperation> unregistered, TrackedBudget budget, ImportOptions options, CancellationToken ct)
    {
        var importResultBuilder = new ImportResultBuilder(detector);

        var imported = streamingOperationRepository.Register(unregistered, budget, ct);
        var operations = imported
            .Select(r => importResultBuilder.Append(r))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value);

        var result = await Update(operations, budget, new(options.TransferConfidenceLevel, TaggingMode.Append), ct);
        importResultBuilder.Append(result);
        return importResultBuilder.Build();
    }

    public async Task<UpdateResult> Update(IAsyncEnumerable<TrackedOperation> operations, TrackedBudget budget, UpdateOptions options, CancellationToken ct)
    {
        var budgets = await Manager.GetOwnedBudgets(ct);
        var errors = new List<IError>();
        var resultBuilder = new UpdateResultBuilder();

        var valid = operations
            .Where(o =>
            {
                if (budgets.Any(b => b.Id == o.Budget.Id)) return true;
                errors.Add(new BudgetDoesNotBelongToCurrentOwnerError()
                    .WithOperationId(o)
                    .WithBudgetId(o.Budget));

                return false;
            });

        var tagged = Retag(valid, budget, options.TaggingMode, ct);

        var saved = streamingOperationRepository.Update(tagged, ct)
            .Select(r => resultBuilder.Append(r))
            .Where(r => r.IsSuccess)
            .Select(r => r.Value);


        var transfers = GetTransfers(saved, budget, options.TransferConfidenceLevel, ct)
            .Select(t => resultBuilder.Append(t))
            .Select(r => r.Value);

        var confident = transfers
            .Where(v => options.TransferConfidenceLevel != null && v.Accuracy >= options.TransferConfidenceLevel)
            .Select(t => new UnregisteredTransfer(
                t.Source as TrackedOperation ?? throw new InvalidOperationException("Attempt was made to register transfer with unregistered transaction!").WithOperationId(t.Source),
                t.Sink as TrackedOperation ?? throw new InvalidOperationException("Attempt was made to register transfer with unregistered transaction!").WithOperationId(t.Sink),
                t.Fee,
                t.Comment,
                t.Accuracy));

        var registrationResult = await RegisterTransfers(confident, ct);
        resultBuilder.Append(registrationResult.Reasons);

        resultBuilder.Append(errors);
        return resultBuilder.Build();
    }

    private async IAsyncEnumerable<TrackedOperation> Retag(IAsyncEnumerable<TrackedOperation> operations, TrackedBudget budget, TaggingMode mode, [EnumeratorCancellation] CancellationToken ct)
    {
        var manager = new TagsManager(budget.TaggingCriteria);
        await foreach (var operation in operations.WithCancellation(ct))
        {
            if (mode != TaggingMode.Skip)
            {
                if (mode == TaggingMode.FromScratch)
                {
                   operation.UntagAll();
                }

                var tags = manager.GetTagsFor(operation);
                foreach (var tag in tags)
                {
                    operation.Tag(tag);
                }
            }

            yield return operation;
        }
    }

    private async IAsyncEnumerable<TrackedTransfer> GetTransfers(IAsyncEnumerable<TrackedOperation> operations, TrackedBudget budget, DetectionAccuracy? tagIf, [EnumeratorCancellation] CancellationToken ct)
    {
        var builder = new TransfersListBuilder(new TransferDetector(budget.TransferCriteria), tagIf);
        await foreach (var operation in operations.WithCancellation(ct))
        {
            var transfer = builder.Add(operation);
            if (transfer is not null)
            {
                yield return transfer;
            }
        }
    }

    public async Task<Result> Remove(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct)
    {
        criteria = await ExtendCriteria(criteria, ct);
        var targets = streamingOperationRepository.Get(criteria, ct);
        var successes = new List<Success>();
        var errors = new List<IError>();

        await foreach(var opResult in streamingOperationRepository.Remove(targets, ct))
        {
            if (opResult.IsSuccess)
            {
                successes.Add(new OperationRemoved(opResult.Value));
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

    public async Task<TransfersList> GetTransfers(DateTime from, DateTime till, TrackedBudget budget, CancellationToken ct)
    {
        var registered = transfersRepository.Get(
            t => t.StartedAt>=from || t.CompletedAt<=till, ct);
        var result = new TransfersList();

        await foreach (var transfer in registered.OrderByDescending(t => t.StartedAt).WithCancellation(ct))
        {
            result.Add(transfer);
        }
        
        var operations = streamingOperationRepository.Get(
            o => o.Timestamp >= from && o.Timestamp <= till && !o.Tags.Contains(TransferTags.Transfer), ct
            ).OrderByDescending(t => t.Timestamp);
        await foreach(var transfer in GetTransfers(operations, budget, null,  ct))
        {
            result.Add(new UnregisteredTransfer(
                (TrackedOperation)transfer.Source, 
                (TrackedOperation)transfer.Sink, 
                transfer.Fee, 
                transfer.Comment, 
                transfer.Accuracy)
            );
        }

        return result;
    }

    public async Task<Result> RegisterTransfers(IAsyncEnumerable<UnregisteredTransfer> transfers, CancellationToken ct)
    {
        var budgets = (await Manager.GetOwnedBudgets(ct)).Select(a => a.Id).ToList();
        var result = new Result();

        var batchSize = 2000;
        var opsQueue = new Queue<TrackedOperation>();
        var xfersQueue = new Queue<TrackedTransfer>();

        await foreach (var transfer in transfers.WithCancellation(ct))
        {
            if (budgets.All(a => a != transfer.Source.Budget.Id))
            {
                result.Reasons.Add(new BudgetDoesNotBelongToCurrentOwnerError().WithBudgetId(transfer.Source.Budget));
                continue;
            }

            if (budgets.All(a => a != transfer.Sink.Budget.Id))
            {
                result.Reasons.Add(new BudgetDoesNotBelongToCurrentOwnerError().WithBudgetId(transfer.Sink.Budget));
                continue;
            }

            transfer.Source.TagSource();
            transfer.Sink.TagSink();

            opsQueue.Enqueue(transfer.Source);
            opsQueue.Enqueue(transfer.Sink);

            xfersQueue.Enqueue(new TrackedTransfer(transfer.Source, transfer.Sink, transfer.Fee, transfer.Comment) {Accuracy = transfer.Accuracy});

            if (xfersQueue.Count > batchSize)
            {
                var results = await streamingOperationRepository.Update(opsQueue.ToAsyncEnumerable(), ct).ToListAsync(ct);
                result.Reasons.AddRange(results.SelectMany(r => r.Reasons));

                var regResults = await transfersRepository.Register(xfersQueue, ct);
                result.Reasons.AddRange(regResults.SelectMany(r => r.Reasons));
            }
        }

        if (xfersQueue.Count > 0)
        {
            var results = await streamingOperationRepository.Update(opsQueue.ToAsyncEnumerable(), ct).ToListAsync(ct);
            result.Reasons.AddRange(results.SelectMany(r => r.Reasons));

            var regResults = await transfersRepository.Register(xfersQueue, ct);
            result.Reasons.AddRange(regResults.SelectMany(r => r.Reasons));
        }

        return result;
    }

    public async Task<Result> RemoveTransfers(IAsyncEnumerable<TrackedTransfer> transfers, CancellationToken ct)
    {
        var successes = new List<ISuccess>();
        var errors = new List<IError>();
        
        var operationsToUpdate = new List<TrackedOperation>();

        await foreach (var transfer in transfers.WithCancellation(ct))
        {
            var result = await transfersRepository.Remove(transfer, ct);
            if (result.IsSuccess)
            {
                successes.Add(new TransferRemoved(transfer));
                foreach (var operation in transfer.Cast<TrackedOperation>())
                {
                    operation.Untag(TransferTags.Transfer);
                    operation.Untag(TransferTags.Source);
                    operation.Untag(TransferTags.Sink);
                    
                    operationsToUpdate.Add(operation);
                }
            }
            else
            {
                errors.AddRange(result.Errors);
            }
        }

        if (operationsToUpdate.Any())
        {
            var budgets = await Manager.GetOwnedBudgets(ct);
            foreach (var operations in operationsToUpdate.GroupBy(o => o.Budget.Id))
            {
                var budget = budgets.FirstOrDefault(b => b.Id == operations.Key);
                if (budget is null)
                {
                    errors.AddRange(operations.Select(o =>
                        new BudgetDoesNotExistError(operations.Key).WithOperationId(o)));
                }
                else
                {
                    var result = await Update(operationsToUpdate.ToAsyncEnumerable(), budget, new (null, TaggingMode.Skip), ct);
                    successes.AddRange(result.Successes);
                    errors.AddRange(result.Errors);
                }
            }
        }

        return new Result().WithSuccesses(successes).WithErrors(errors);
    }
}
