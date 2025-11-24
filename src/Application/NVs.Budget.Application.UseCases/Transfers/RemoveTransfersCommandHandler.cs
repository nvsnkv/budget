using System.Linq.Expressions;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.UseCases.Transfers;

internal class RemoveTransfersCommandHandler(ITransfersRepository repository) : IRequestHandler<RemoveTransfersCommand, Result>
{
    public async Task<Result> Handle(RemoveTransfersCommand request, CancellationToken cancellationToken)
    {
        var result = new Result();
        Expression<Func<TrackedTransfer,bool>> filter = request.All
            ? t => true :
            t => request.SourceIds.Contains(t.Source.Id);

        var targets = repository.Get(filter, cancellationToken);
        var items = await targets.ToListAsync(cancellationToken);

        foreach (var item in items)
        {
            var r = await repository.Remove(item, cancellationToken);
            result.WithReasons(r.Reasons);
        }

        return result;
    }
}
