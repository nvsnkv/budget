using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Operations;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Input;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Operations;

[Verb("update", HelpText = "Updates tracked operations from the given input")]
internal class UpdateVerb : CriteriaBasedOperationsVerb
{
    [Option('f', "file", HelpText = "Path to file with update content. If value is not specified, app will use standard input")]
    public string? FilePath { get; set; }
}

internal class UpdateVerbHandler(IMediator mediator, IInputStreamProvider streams, IOperationsReader reader, CriteriaParser parser, IResultWriter<Result> writer) : CriteriaBasedOperationsVerbHandler<UpdateVerb>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(UpdateVerb request, Expression<Func<TrackedOperation, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var steamReader = await streams.GetInput(request.FilePath ?? string.Empty);
        if (!steamReader.IsSuccess)
        {
            await Writer.Write(steamReader.ToResult(), cancellationToken);
            return ExitCode.ArgumentsError;
        }


        var exitCodes = new HashSet<ExitCode>();
        var operations = reader.ReadTrackedOperation(steamReader.Value, cancellationToken).SelectAwait(async r =>
        {
            if (r.IsSuccess) return r.Value;
            await Writer.Write(r.ToResult(), cancellationToken);
            exitCodes.Add(r.ToExitCode());
            return null;
        }).Where(o => o is not null);

        var result = await mediator.Send(new UpdateOperationsCommand(operations!), cancellationToken);
        exitCodes.Add(result.ToExitCode());

        return exitCodes.Aggregate((r, e) => r | e);
    }
}
