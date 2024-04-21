using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Controllers.Console.IO.Models;

namespace NVs.Budget.Controllers.Console.IO.Output.Operations;

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
