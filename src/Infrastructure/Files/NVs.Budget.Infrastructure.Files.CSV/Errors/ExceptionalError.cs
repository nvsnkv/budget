using FluentResults;

namespace NVs.Budget.Infrastructure.Files.CSV.Errors;

internal class ConversionError(Exception exception) : IError
{
    public string Message => exception.Message;
    public Dictionary<string, object> Metadata { get; } = new() { { nameof(exception), exception.ToString() } };
    public List<IError> Reasons { get; } = new();
}
