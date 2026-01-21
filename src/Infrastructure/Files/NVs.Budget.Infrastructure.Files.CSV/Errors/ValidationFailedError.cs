using FluentResults;

namespace NVs.Budget.Infrastructure.Files.CSV.Errors;

internal class ValidationFailedError(string errorMessage) : IError
{
    public string Message { get; } = errorMessage;
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}

