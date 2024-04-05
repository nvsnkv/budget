using CommandLine;
using FluentResults;
using JetBrains.Annotations;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Owners;
using NVs.Budget.Controllers.Console.Criteria;
using NVs.Budget.Controllers.Console.IO;

namespace NVs.Budget.Controllers.Console.Commands.Owners;

[Verb("list", isDefault: true, HelpText = "Produces list of owners, tracked by system")]
internal class ListOwnersVerb : IRequest<int>
{
    [Option('p', "param-name", HelpText = "Criteria parameter name", Default = "o")]
    public string ParamName { get; [UsedImplicitly] set; }

    [Value(0)] public IEnumerable<string> Criteria { get; [UsedImplicitly] set; }
};

internal class ListOwnersVerbHandler(IMediator mediator, CriteriaParser parser, ResultWriter<Result> resultWriter) : IRequestHandler<ListOwnersVerb, int>
{
    public async Task<int> Handle(ListOwnersVerb request, CancellationToken cancellationToken)
    {
        var expression = request.Criteria.Aggregate(string.Empty, (a,i) => a + " " + i);
        var criteria = parser.TryParsePredicate<TrackedOwner>(expression, request.ParamName);
        if (criteria.IsFailed)
        {
            await resultWriter.Write(criteria.ToResult(), cancellationToken);
            return (int)ExitCodes.ArgumentsError;
        }

        // CLI output will be handled by ResultWritingBehaviour
        await mediator.Send(new ListOwnersQuery(criteria.Value), cancellationToken);

        return (int)ExitCodes.Success;
    }
}
