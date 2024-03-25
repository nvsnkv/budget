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

    public async Task<IReadOnlyCollection<TrackedTransfer>> Get(Expression<Func<TrackedTransfer, bool>> filter, CancellationToken ct)
    {
        var expression = filter.ConvertTypes<TrackedTransfer, StoredTransfer>(MappingProfile.TypeMappings);
        expression = expression.CombineWith(a => !a.Deleted);

        var (queryable, enumerable) = _splitter.Split(expression);
        var query = context.Transfers.Include(t => t.Source).ThenInclude(o => o.Account).ThenInclude(a => a.Owners)
            .Include(t => t.Sink).ThenInclude(o => o.Account).ThenInclude(a => a.Owners)
            .Where(queryable);

        var items = await query.AsNoTracking().ToListAsync(ct);
        items = items.Where(enumerable).ToList();

        return mapper.Map<List<TrackedTransfer>>(items).AsReadOnly();
    }

    public async Task<Result> Register(TrackedTransfer transfer, CancellationToken ct)
    {
        var source = await context.Operations.Where(o => o.Id == transfer.Source.Id).FirstOrDefaultAsync(ct);
        if (source is null)
        {
            return Result.Fail(new EntityDoesNotExistError<Operation>(transfer.Source));
        }

        var sink = await context.Operations.Where(o => o.Id == transfer.Sink.Id).FirstOrDefaultAsync(ct);
        if (sink is null)
        {
            return Result.Fail(new EntityDoesNotExistError<Operation>(transfer.Sink));
        }

        var existing = await context.Transfers.Where(t => t.Source.Id == transfer.Source.Id && t.Sink.Id == transfer.Sink.Id).FirstOrDefaultAsync(ct);
        if (existing is not null)
        {
            return Result.Fail(new TransferAlreadyRegisteredError(transfer));
        }

        await context.Transfers.AddAsync(new StoredTransfer(transfer.Comment)
        {
            Fee = mapper.Map<StoredMoney>(transfer.Fee),
            Source = source,
            Sink = sink
        }, ct);

        await context.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
