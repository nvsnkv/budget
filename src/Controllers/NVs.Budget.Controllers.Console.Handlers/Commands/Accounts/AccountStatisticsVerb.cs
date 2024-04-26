using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Commands.Operations;
using NVs.Budget.Controllers.Console.Handlers.Criteria;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("stats", HelpText = "Computes statistics for accounts that matches criteria within date range")]
internal class AccountStatisticsVerb : CriteriaBasedVerb
{
    [Option('p', "logbook-path", Required = true, HelpText = "A path to logbook to write")]
    public string LogbookPath { get; set; } = string.Empty;

    [Option('f', "from", HelpText = "Date from")]
    public DateTime From { get; set; } = DateTime.MinValue;

    [Option('t', "till", HelpText = "Date till")]
    public DateTime Till { get; set; } = DateTime.MaxValue;

    [Option("with-counts", Default = true, HelpText = "Write operations count for each account")]
    public bool WithCounts { get; set; }

    [Option("with-amount", HelpText = "Write amounts for each account")]
    public bool WithAmounts { get; set; }

    [Option("with-operations", HelpText = "Write list of operations for each account")]
    public bool WithOperations { get; set; }
}

internal class AccountStatisticsVerbHandler(
    IMediator mediator,
    CriteriaParser parser,
    IResultWriter<Result> writer,
    ILogbookWriter logbookWriter
) : CriteriaBasedVerbHandler<AccountStatisticsVerb, TrackedAccount>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(AccountStatisticsVerb request, Expression<Func<TrackedAccount, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var query = new CalcAccountStatisticsQuery(criteriaResultValue, o => o.Timestamp  >= request.From && o.Timestamp < request.Till);
        var result = await mediator.Send(query, cancellationToken);

        var range = new NamedRange(string.Empty, request.From, request.Till);
        await logbookWriter.Write(result.ValueOrDefault,
            new LogbookWritingOptions(
                request.LogbookPath,
                request.WithCounts,
                request.WithAmounts,
                request.WithOperations,
                Enumerable.Repeat(range, 1)),
            cancellationToken
            );

        return result.ToExitCode();
    }
}
