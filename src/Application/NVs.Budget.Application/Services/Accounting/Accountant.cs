using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Contracts.Results;
using NVs.Budget.Application.Contracts.Services;
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
    IOperationsRepository operationsRepository,
    ITransfersRepository transfersRepository,
    IBudgetManager manager,
    TagsManager tagsManager,
    TransfersListBuilder transfersListBuilder,
    ImportResultBuilder importResultBuilder) :ReckonerBase(manager), IAccountant
{
    public async Task<ImportResult> ImportOperations(IAsyncEnumerable<UnregisteredOperation> transactions, ImportOptions options, CancellationToken ct)
    {
        importResultBuilder.Clear();
        transfersListBuilder.Clear();

        var accounts = (await Manager.GetOwnedBudgets(ct)).ToList();

        await foreach (var unregisteredTransaction in transactions.WithCancellation(ct))
        {
            var accountResult = await TryGetAccount(accounts, unregisteredTransaction.Budget, options.RegisterAccounts, ct);
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

    public async Task<Result> Update(IAsyncEnumerable<TrackedOperation> operations, CancellationToken ct)
    {
        var accounts = await Manager.GetOwnedBudgets(ct);
        var result = new Result();
        await foreach (var transaction in operations.WithCancellation(ct))
        {
            var updateResult = await Update(transaction, accounts, ct);
            result.Reasons.AddRange(updateResult.Reasons);
        }

        return result;
    }

    public Task<Result> Retag(IAsyncEnumerable<TrackedOperation> operations, bool fromScratch, CancellationToken ct)
    {
        var updated = operations.Select(operation =>
        {
            if (fromScratch)
            {
                foreach (var tag in operation.Tags)
                {
                    operation.Untag(tag);
                }
            }

            var tags = tagsManager.GetTags(operation);
            foreach (var tag in tags.Except(operation.Tags))
            {
                operation.Tag(tag);
            }

            return operation;
        });

        return Update(updated, ct);
    }

    public async Task<Result> Remove(Expression<Func<TrackedOperation, bool>> criteria, CancellationToken ct)
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
        var accounts = await Manager.GetOwnedBudgets(ct);
        var result = new Result();

        await foreach (var transfer in transfers.WithCancellation(ct))
        {
            if (accounts.All(a => a != transfer.Source.Budget))
            {
                result.Reasons.Add(new AccountDoesNotBelongToCurrentOwnerError().WithAccountId(transfer.Source.Budget));
                continue;
            }

            if (accounts.All(a => a != transfer.Sink.Budget))
            {
                result.Reasons.Add(new AccountDoesNotBelongToCurrentOwnerError().WithAccountId(transfer.Sink.Budget));
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

    private async Task<Result<TrackedOperation>> Update(TrackedOperation operation, IReadOnlyCollection<TrackedBudget> ownedAccounts, CancellationToken ct)
    {
        if (ownedAccounts.All(a => operation.Budget != a))
        {
            return Result.Fail(new AccountDoesNotBelongToCurrentOwnerError()
                .WithTransactionId(operation)
                .WithAccountId(operation.Budget));
        }

        var updateResult = await operationsRepository.Update(operation, ct);
        return updateResult.IsSuccess
            ? Result.Ok(updateResult.Value).WithReason(new OperationUpdated(updateResult.Value))
            : Result.Fail(updateResult.Errors);
    }

    private async Task<Result<TrackedBudget>> TryGetAccount(ICollection<TrackedBudget> accounts, UnregisteredBudget budget, bool shouldRegister, CancellationToken ct)
    {
        var registeredAccount = accounts.FirstOrDefault(a => a.Name == budget.Name);
        if (registeredAccount is not null) return Result.Ok(registeredAccount);

        if (!shouldRegister)
        {
            return Result.Fail(new AccountNotFoundError()
                .WithMetadata(nameof(TrackedBudget.Name), budget.Name)
            );
        }
        var result = await Manager.Register(budget, ct);
        if (!result.IsSuccess)
        {
            return Result.Fail(result.Errors);
        }

        registeredAccount = result.Value;
        accounts.Add(registeredAccount);

        return Result.Ok(registeredAccount).WithReason(new AccountAdded(registeredAccount));
    }
}
