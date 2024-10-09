using System.Linq.Expressions;
using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Handlers.Criteria;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal abstract class StatisticsVerbHandlerBase<T, TCriteria>(
    CriteriaParser parser,
    ILogbookWriter logbookWriter,
    IResultWriter<Result> writer,
    CronBasedNamedRangeSeriesBuilder seriesBuilder,
    string criteriaParamName = "o") : CriteriaBasedVerbHandler<T, TCriteria>(parser, writer, criteriaParamName) where T: StatisticsVerb
{
    private Result<IEnumerable<NamedRange>> GetRanges(DateTime from, DateTime till, string? schedule)
    {
        return string.IsNullOrEmpty(schedule)
            ? new NamedRange[]{ new (string.Empty, from, till) }
            : seriesBuilder.GetRanges(from, till, schedule);
    }

    protected override async Task<ExitCode> HandleInternal(T request, Expression<Func<TCriteria, bool>> criteriaResultValue, CancellationToken cancellationToken)
    {
        var ranges = GetRanges(request.From, request.Till, request.Schedule);
        await Writer.Write(ranges.ToResult(), cancellationToken);
        if (!ranges.IsSuccess)
        {
            return ranges.ToExitCode();
        }

        var result = await GetLogbook(request, criteriaResultValue, cancellationToken);

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

    protected abstract Task<Result<CriteriaBasedLogbook>> GetLogbook(T request, Expression<Func<TCriteria, bool>> criteriaResultValue, CancellationToken ct);
}
