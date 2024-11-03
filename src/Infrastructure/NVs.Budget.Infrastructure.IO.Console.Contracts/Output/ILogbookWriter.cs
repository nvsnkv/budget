using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output;

public interface ILogbookWriter
{
    public Task Write(CriteriaBasedLogbook? logbook, LogbookWritingOptions options, CancellationToken ct);
}
