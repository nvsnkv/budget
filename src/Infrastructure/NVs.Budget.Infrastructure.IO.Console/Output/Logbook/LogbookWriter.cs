using AutoMapper;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Infrastructure.IO.Console.Options;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Logbook;

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
