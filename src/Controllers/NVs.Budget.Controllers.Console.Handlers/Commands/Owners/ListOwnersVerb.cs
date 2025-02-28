using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Owners;

[Verb("list", isDefault: true, HelpText = "Produces list of owners, tracked by system")]
internal class ListOwnersVerb : AbstractVerb
{
    [Option('p', "param-name", HelpText = "Criteria parameter name", Default = "o")]
    public string ParamName { get; [UsedImplicitly] set; } = "o";

    [Value(0)] public IEnumerable<string>? Criteria { get; [UsedImplicitly] set; }
}

[UsedImplicitly]
internal class ListOwnersVerbHandler(IMediator mediator, ICriteriaParser parser, IResultWriter<Result> resultWriter, IObjectWriter<TrackedOwner> writer) : IRequestHandler<ListOwnersVerb, ExitCode>
{
    public async Task<ExitCode> Handle(ListOwnersVerb request, CancellationToken cancellationToken)
    {
        var expression = request.Criteria?.Aggregate(string.Empty, (a,i) => a + " " + i) ?? string.Empty;
        var criteria = parser.TryParsePredicate<TrackedOwner>(expression, request.ParamName);
        if (criteria.IsFailed)
        {
            await resultWriter.Write(criteria.ToResult(), cancellationToken);
            return ExitCode.ArgumentsError;
        }

        var owners = await mediator.Send(new ListOwnersQuery(criteria.Value), cancellationToken);
        foreach (var owner in owners)
        {
            await writer.Write(owner, cancellationToken);
        }

        return ExitCode.Success;
    }
}
