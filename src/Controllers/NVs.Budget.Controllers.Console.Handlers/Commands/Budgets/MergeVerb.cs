using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Infrastructure.IO.Console.Output;
using Error = FluentResults.Error;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Budgets;

[Verb("merge", HelpText = "Merges budgets into the last one")]
internal class MergeVerb : AbstractVerb
{
    [Value(0, MetaName = "Budget ids to merge (min 2). All operations will be moved from budgets into the last one provided")]
    public IEnumerable<string> BudgetIds { get; set; } = Enumerable.Empty<string>();

    [Option("purge", Default = false, HelpText = "Purge empty accounts")]
    public bool Purge { get; set; }
}

internal class MergeVerbHandler(IMediator mediator, IResultWriter<Result> resultWriter) : IRequestHandler<MergeVerb, ExitCode>
{
    public async Task<ExitCode> Handle(MergeVerb request, CancellationToken cancellationToken)
    {
        var ids = new List<Guid>();
        foreach (var budgetId in request.BudgetIds)
        {
            if (!Guid.TryParse(budgetId, out var i))
            {
                var parseResult = Result.Fail(new Error("Failed to parse Guid").WithMetadata("Value", budgetId));
                await resultWriter.Write(parseResult, cancellationToken);
                return ExitCode.ArgumentsError;
            }

            ids.Add(i);
        }

        var rq = new MergeAccountsRequest(ids, request.Purge);
        var result = await mediator.Send(rq, cancellationToken);
        return result.ToExitCode();
    }
}
