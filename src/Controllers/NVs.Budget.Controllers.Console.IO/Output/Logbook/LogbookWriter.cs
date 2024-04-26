using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Console.IO.Output.Logbook;

internal class LogbookWriter(IOutputStreamProvider streams) : ILogbookWriter
{
    public async Task Write(CriteriaBasedLogbook? logbook, LogbookWritingOptions options, CancellationToken ct)
    {
        if (logbook is null)
        {
            return;
        }

        var streamWriter = await streams.GetOutput(options.Path);
        var stream = streamWriter.BaseStream;

        var workbook = new CriteriaBasedXLLogbook(logbook, stream, options);
        workbook.Save();
    }
}
