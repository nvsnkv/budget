using FluentResults;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Input.CsvTransfersReader.Errors;

internal class SourceNotFoundError(Guid sourceId) : IError
{
    public string Message { get; } = "Source operation not found!";
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(UnregisteredTransfer.Source), sourceId } };
    public List<IError> Reasons { get; } = new();
}
