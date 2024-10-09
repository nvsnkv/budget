using System.Linq.Expressions;
using CommandLine;
using FluentResults;
using MediatR;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.UseCases.Accounts;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Controllers.Console.Handlers.Criteria;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Console.Handlers.Commands.Accounts;

[Verb("stats", HelpText = "Computes statistics for accounts that matches criteria within date range")]
internal class AccountStatisticsVerb : StatisticsVerb
{
}

internal class AccountStatisticsVerbHandler(
    IMediator mediator,
    CriteriaParser parser,
    IResultWriter<Result> writer,
    ILogbookWriter logbookWriter,
    CronBasedNamedRangeSeriesBuilder seriesBuilder
) : StatisticsVerbHandlerBase<AccountStatisticsVerb, TrackedBudget>(parser, logbookWriter, writer, seriesBuilder, "a")
{
    protected override Task<Result<CriteriaBasedLogbook>> GetLogbook(AccountStatisticsVerb request, Expression<Func<TrackedBudget, bool>> criteriaResultValue, CancellationToken ct)
    {
        var query = new CalcAccountStatisticsQuery(
            criteriaResultValue,
            o => o.Timestamp  >= request.From.ToUniversalTime() && o.Timestamp < request.Till.ToUniversalTime()
        );

        return mediator.Send(query, ct);
    }
}
