using CommandLine;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.Identity.Contracts;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Owners;

[Verb("self-register")]
internal class SelfRegisterVerb : IRequest<ExitCode> { }

[UsedImplicitly]
internal class SelfRegisterVerbHandler(IMediator mediator, IIdentityService identityService) : IRequestHandler<SelfRegisterVerb, ExitCode>
{
    public async Task<ExitCode> Handle(SelfRegisterVerb request, CancellationToken cancellationToken)
    {
        var user = await identityService.GetCurrentUser(cancellationToken);

        var result = await mediator.Send(new RegisterOwnerCommand(user), cancellationToken);

        return result.ToExitCode();
    }
}
