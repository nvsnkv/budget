using FluentResults;

namespace NVs.Budget.Infrastructure.Files.CSV.Errors;

internal class AttributeParsingError(string attributeName) : IError
{
    public string Message { get; } = $"Failed to parse attribute {attributeName}";
    public Dictionary<string, object> Metadata { get; } = new();
    public List<IError> Reasons { get; } = new();
}

