using FluentResults;

namespace NVs.Budget.Utilities.Yaml;

public class YamlParsingError(string reason, IEnumerable<string> path) : IError
{
    public string Message { get; } = reason;
    public Dictionary<string, object> Metadata { get; } = new() { {"Path", string.Join('.', path) } };
    public List<IError> Reasons { get; } = new();
}