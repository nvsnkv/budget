using AutoMapper;
using NVs.Budget.Controllers.Console.Contracts.IO.Options;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;
using NVs.Budget.Domain.Aggregates;

namespace NVs.Budget.Controllers.Console.IO.Output.Logbook;

internal class LogbookWriter(IOutputStreamProvider streams, IMapper mapper) : ILogbookWriter
{
    public async Task Write(CriteriaBasedLogbook? logbook, LogbookWritingOptions options, CancellationToken ct)
    {
        if (logbook is null)
        {
            return;
        }

        if (File.Exists(options.Path))
        {
            File.Delete(options.Path);
        }

        var streamWriter = await streams.GetOutput(options.Path);
        var stream = streamWriter.BaseStream;

        var workbook = new CriteriaBasedXLLogbook(logbook, options, mapper);
        workbook.SaveTo(stream);
        await stream.FlushAsync(ct);
    }
}
