using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Application.UseCases.Owners;

internal class RegisterOwnerHandler(IOwnersRepository repository) : IRequestHandler<RegisterOwnerCommand, Result<TrackedOwner>>
{
    public async Task<Result<TrackedOwner>> Handle(RegisterOwnerCommand request, CancellationToken cancellationToken)
    {
        var owner = await repository.Get(request.User, cancellationToken);
        if (owner is not null)
        {
            return owner;
        }

        return await repository.Register(request.User, cancellationToken);
    }
}
