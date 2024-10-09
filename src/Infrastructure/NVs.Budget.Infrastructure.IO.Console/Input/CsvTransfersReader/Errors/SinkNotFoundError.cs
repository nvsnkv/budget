using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvTransfersReader.Errors;

internal class SinkNotFoundError(Guid sinkId) : IError
{
    public string Message { get; } = "Sink operation not found!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(UnregisteredTransfer.Sink), sinkId } };
    public List<IError> Reasons { get; } = new();
}