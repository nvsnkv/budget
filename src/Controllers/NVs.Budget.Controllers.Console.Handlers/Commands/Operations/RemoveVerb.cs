using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("remove", HelpText = "Removes operations that matches criteria")]
internal class RemoveVerb : CriteriaBasedVerb;

internal class RemoveVerbHandler(IMediator mediator, CriteriaParser parser, IResultWriter<Result> writer) : CriteriaBasedVerbHandler<RemoveVerb, TrackedOperation>(parser, writer)
{
    protected override  async Task<ExitCode> HandleInternal(RemoveVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new RemoveOperationsCommand(criteriaResultValue), cancellationToken);
        return result.ToExitCode();
    }
}
