using System.Linq.Expressions;
using AutoMapper;
using FluentResults;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.ExpressionVisitors;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories.Results;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Infrastructure.Persistence.EF.Repositories;

internal class TransfersRepository(IMapper mapper, BudgetContext context) : ITransfersRepository
{
    private readonly ExpressionSplitter _splitter = new();

    public IAsyncEnumerable<TrackedTransfer> Get(Expression<Func<TrackedTransfer, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedTransfer, StoredTransfer>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var (queryable, enumerable) = _splitter.Split(expression);
        return context.Transfers
            .Include(t => t.Fee)
            .Include(t => t.Source).ThenInclude(o => o.Budget).ThenInclude(a => a.Owners)
            .Include(t => t.Sink).ThenInclude(o => o.Budget).ThenInclude(a => a.Owners)
            .AsNoTracking()
            .Where(queryable)
            .AsSplitQuery()
            .AsAsyncEnumerable()
            .Where(enumerable)
            .Select(mapper.Map<TrackedTransfer>);
    }

    public async Task<IEnumerable<Result>> Register(IReadOnlyCollection<TrackedTransfer> transfers, CancellationToken ct)
    {
        var ids = transfers.SelectMany(t => t).Select(t => t.Id).ToList();
        var operations = await context.Operations.Where(o => ids.Contains(o.Id)).ToListAsync(ct);
        var existing = await context.Transfers
            .Where(t => ids.Contains(t.Source.Id) || ids.Contains(t.Sink.Id))
            .ToDictionaryAsync(t => (t.Source.Id, t.Sink.Id), ct);

        var results = new List<Result>();

        foreach (var transfer in transfers)
        {
            var source = operations.FirstOrDefault(o => o.Id == transfer.Source.Id);
            if (source is null)
            {
                results.Add(Result.Fail(new EntityDoesNotExistError<Operation>(transfer.Source)));
                continue;
            }

            var sink = operations.FirstOrDefault(o => o.Id == transfer.Sink.Id);
            if (sink is null)
            {
                results.Add(Result.Fail(new EntityDoesNotExistError<Operation>(transfer.Sink)));
                continue;
            }

            if (existing.ContainsKey((source.Id, sink.Id)))
            {
                results.Add(Result.Fail(new TransferAlreadyRegisteredError(transfer)));
                continue;
            }

            context.Transfers.Add(new StoredTransfer(transfer.Comment)
            {
                Fee = mapper.Map<StoredMoney>(transfer.Fee),
                Source = source,
                Sink = sink
            });

            results.Add(Result.Ok());
        }

        await context.SaveChangesAsync(ct);
        return results;
    }

    public async Task<Result> Remove(TrackedTransfer transfer, CancellationToken ct)
    {
        var target = await context.Transfers.FirstOrDefaultAsync(t =>
            t.Source.Id == transfer.Source.Id
            && t.Sink.Id == transfer.Sink.Id
            && t.Deleted == false, ct);

        if (target is null) return Result.Fail(new EntityDoesNotExistError<TrackedTransfer>(transfer));

        context.Transfers.Remove(target);
        await context.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
