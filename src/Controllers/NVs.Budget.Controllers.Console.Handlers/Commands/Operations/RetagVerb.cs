using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("retag", HelpText = "Retags operations using tagging criteria from config")]
internal class RetagVerb : CriteriaBasedVerb
{
    [Option("--from-scratch", HelpText = "Clear old tags")]
    public bool FromScratch { get; set; }
}

internal class RetagVerbHandler(IMediator mediator, ICriteriaParser parser, IResultWriter<Result> writer) : CriteriaBasedVerbHandler<RetagVerb, TrackedOperation>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(RetagVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var command = new RetagOperationsCommand(criteriaResultValue, request.FromScratch);
        var result = await mediator.Send(command, cancellationToken);
        return result.ToExitCode();
    }
}
