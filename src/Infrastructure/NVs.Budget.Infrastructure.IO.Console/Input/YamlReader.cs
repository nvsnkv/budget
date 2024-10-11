using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace NVs.Budget.Infrastructure.IO.Console.Input;

internal abstract class YamlReader
{
    protected static readonly string[] EmptyPath = [];
    protected Result<YamlMappingNode> LoadRootNodeFrom(StreamReader reader)
    {
        var stream = new YamlStream();
        try { stream.Load(reader); } catch(YamlException e) { return Result.Fail(new ExceptionBasedError(e, e.ToString())); }

        var count = stream.Documents.Count;
        if (count != 1)
        {
            return Result.Fail(new YamlParsingError(count == 0 ? "No YAML document found in input" : "Multiple documents found in input", EmptyPath));
        }

        var document = stream.Documents.Single();

        if (document.RootNode is not YamlMappingNode mapping)
        {
            return Result.Fail(new UnexpectedNodeTypeError(document.RootNode.GetType(), typeof(YamlMappingNode), EmptyPath));
        }

        return mapping;
    }

    protected Result<string> ReadString(YamlNode node, ICollection<string> path)
    {
        if (node is not YamlScalarNode scalar)
        {
            return Result.Fail(new UnexpectedNodeTypeError(node.GetType(), typeof(YamlScalarNode), path));
        }

        return scalar.Value ?? string.Empty;
    }
}
