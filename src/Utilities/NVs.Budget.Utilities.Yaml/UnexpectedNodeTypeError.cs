namespace NVs.Budget.Utilities.Yaml;

public class UnexpectedNodeTypeError : YamlParsingError
{
    public UnexpectedNodeTypeError(Type type, Type expected, ICollection<string> path) : base("Unexpected node type found", path)
    {
        Metadata.Add("Key", path.LastOrDefault() ?? string.Empty);
        Metadata.Add("Expected", expected.Name);
        Metadata.Add("Type", type.Name);
    }
}