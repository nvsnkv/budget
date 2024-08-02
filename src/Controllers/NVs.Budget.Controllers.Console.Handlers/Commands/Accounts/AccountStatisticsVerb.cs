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
using NVs.Budget.Controllers.Console.Handlers.Utils;

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

    [Option('s', "schedule", HelpText = "Cron expression to generate time ranges. If not set, all values will be accumulated in a single time range between From and Till")]
    public string? Schedule { get; set; }

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
    ILogbookWriter logbookWriter,
    CronBasedNamedRangeSeriesBuilder seriesBuilder
) : CriteriaBasedVerbHandler<AccountStatisticsVerb, TrackedAccount>(parser, writer)
{
    protected override async Task<ExitCode> HandleInternal(AccountStatisticsVerb request, Expression<Func<TrackedAccount, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var query = new CalcAccountStatisticsQuery(criteriaResultValue, o => o.Timestamp  >= request.From && o.Timestamp < request.Till);
        var result = await mediator.Send(query, cancellationToken);

        var ranges = GetRanges(request.From, request.Till, request.Schedule);
        await Writer.Write(ranges.ToResult(), cancellationToken);
        if (!ranges.IsSuccess)
        {
            return ranges.ToExitCode();
        }

        await logbookWriter.Write(result.ValueOrDefault,
            new LogbookWritingOptions(
                request.LogbookPath,
                request.WithCounts,
                request.WithAmounts,
                request.WithOperations,
                ranges.Value),
            cancellationToken
            );

        return result.ToExitCode();
    }

    private Result<IEnumerable<NamedRange>> GetRanges(DateTime from, DateTime till, string? schedule)
    {
        return string.IsNullOrEmpty(schedule)
            ? new NamedRange[]{ new (string.Empty, from, till) }
            : seriesBuilder.GetRanges(from, till, schedule);
    }
}
