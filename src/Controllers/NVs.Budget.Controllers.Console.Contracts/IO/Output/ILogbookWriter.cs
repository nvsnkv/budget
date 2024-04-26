using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Console.Contracts.IO.Output;

public interface ILogbookWriter
{
    public Task Write(CriteriaBasedLogbook? logbook, LogbookWritingOptions options, CancellationToken ct);
}
