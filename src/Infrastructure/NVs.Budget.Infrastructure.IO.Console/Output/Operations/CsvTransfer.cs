using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Infrastructure.IO.Console.Models;

namespace NVs.Budget.Infrastructure.IO.Console.Output.Operations;

internal class CsvTransfer
{
    public Guid SourceId { get; init; }
    public Guid SinkId { get; init; }
    public string? Fee { get; init; }
    public DetectionAccuracy Accuracy { get; init; }
    public string? Comment { get; init; }

    public CsvTrackedOperation? Source { get; init; }
    public CsvTrackedOperation? Sink { get; init; }
}
