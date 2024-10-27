using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Transfers;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.UseCases.Transfers;

internal class RemoveTransfersCommandHandler(ITransfersRepository repository) : IRequestHandler<RemoveTransfersCommand, Result>
{
    public async Task<Result> Handle(RemoveTransfersCommand request, CancellationToken cancellationToken)
    {
        var result = new Result();
        var targets = await repository.Get(t => request.SourceIds.Contains(t.Source.Id), cancellationToken).ToListAsync(cancellationToken);

        foreach (var target in targets)
        {
            var r = await repository.Remove(target, cancellationToken);
            result.WithReasons(r.Reasons);
        }

        return result;
    }
}
