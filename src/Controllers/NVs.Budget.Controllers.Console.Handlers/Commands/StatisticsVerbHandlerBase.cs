using FluentResults;
using MediatR;
using NVs.Budget.Controllers.Console.Contracts.Commands;
using NVs.Budget.Controllers.Console.Handlers.Utils;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Options;
using NVs.Budget.Infrastructure.IO.Console.Output;

namespace NVs.Budget.Controllers.Console.Handlers.Commands;

internal abstract class StatisticsVerbHandlerBase<T>(
    ILogbookWriter logbookWriter,
    IResultWriter<Result> writer,
    CronBasedNamedRangeSeriesBuilder seriesBuilder,
    IOutputStreamProvider outputs,
    OutputOptions options) : IRequestHandler<T,ExitCode> where T : StatisticsVerb
{
    private Result<IEnumerable<NamedRange>> GetRanges(DateTime from, DateTime till, string? schedule)
    {
        return string.IsNullOrEmpty(schedule)
            ? new NamedRange[]{ new (string.Empty, from, till) }
            : seriesBuilder.GetRanges(from, till, schedule);
    }

    public async Task<ExitCode> Handle(T request, CancellationToken cancellationToken)
    {


        var ranges = GetRanges(request.From, request.Till, request.Schedule);
        await writer.Write(ranges.ToResult(), cancellationToken);
        if (!ranges.IsSuccess)
        {
            return ranges.ToExitCode();
        }

        var output = await outputs.GetOutput(options.OutputStreamName);
        await output.WriteLineAsync($"Logbook: {request.LogbookPath}");

        var result = await GetLogbook(request, cancellationToken);
        await writer.Write(result.ToResult(), cancellationToken);

        await logbookWriter.Write(result.ValueOrDefault,
            new LogbookWritingOptions(
                request.LogbookPath,
                request.WithCounts,
                request.WithOperations,
                ranges.Value),
            cancellationToken
        );

        return result.ToExitCode();
    }

    protected abstract Task<Result<CriteriaBasedLogbook>> GetLogbook(T request, CancellationToken ct);
}
