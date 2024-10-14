using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.IO.Console.Input.CsvTransfersReader.Errors;

internal class SourceNotFoundError(Guid sourceId) : IError
{
    public string Message { get; } = "Source operation not found!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(UnregisteredTransfer.Source), sourceId } };
    public List<IError> Reasons { get; } = new();
}
