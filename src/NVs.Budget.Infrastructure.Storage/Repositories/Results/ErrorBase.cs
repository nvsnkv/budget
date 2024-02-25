using FluentResults;

namespace NVs.Budget.Infrastructure.Storage.Repositories.Results;

internal class ErrorBase(string message, Dictionary<string, object>? meta = null) : IError
{
    public string Message { get; } = message;
    public Dictionary<string, object> Metadata { get; } = meta ?? new();
    public List<IError> Reasons { get; } = new();
}
