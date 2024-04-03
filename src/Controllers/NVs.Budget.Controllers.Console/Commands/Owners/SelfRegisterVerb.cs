using CommandLine;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Infrastructure.Identity.Contracts;

namespace NVs.Budget.Controllers.Console.Commands.Owners;

[Verb("self-register")]
internal class SelfRegisterVerb : IRequest<int> { }

internal class SelfRegisterVerbHandler(IMediator mediator, IIdentityService identityService) : IRequestHandler<SelfRegisterVerb, int>
{
    public async Task<int> Handle(SelfRegisterVerb request, CancellationToken cancellationToken)
    {
        var user = await identityService.GetCurrentUser(cancellationToken);

        var result = await mediator.Send(new RegisterOwnerCommand(user), cancellationToken);

        return result.ToExitCode();
    }
}
