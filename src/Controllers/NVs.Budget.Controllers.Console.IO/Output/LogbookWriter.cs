using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Console.IO.Output;

internal class LogbookWriter : ILogbookWriter
{
    public Task Write(CriteriaBasedLogbook? logbook, LogbookWritingOptions options, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
