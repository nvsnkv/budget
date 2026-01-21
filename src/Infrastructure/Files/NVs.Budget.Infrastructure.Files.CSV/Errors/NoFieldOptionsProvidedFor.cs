using FluentResults;

namespace NVs.Budget.Infrastructure.Files.CSV.Errors;

internal class NoFieldOptionsProvidedFor(string name) : IError
{
    public string Message { get; } = $"No field options provided for {name}";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}

