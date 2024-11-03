using FluentResults;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Utilities.Expressions;
using YamlDotNet.RepresentationModel;
using Tag = NVs.Budget.Domain.ValueObjects.Tag;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;

internal class YamlBasedLogbookCriteriaReader(ReadableExpressionsParser parser) : ILogbookCriteriaReader
{
    private static readonly string[] EmptyPath = [];
    private static readonly YamlScalarNode CriteriaKey = new("criteria");
    private static readonly YamlScalarNode TagsKey = new("tags");
    private static readonly YamlScalarNode TagsModeKey = new("type");
    private static readonly YamlScalarNode SubcriteriaKey = new("subcriteria");
    private static readonly YamlScalarNode SubstitutionKey = new("substitution");

    public Task<Result<LogbookCriteria>> ReadFrom(StreamReader reader, CancellationToken ct)
    {
        var stream = new YamlStream();
        try { stream.Load(reader); } catch(Exception e) { return Task.FromResult(Result.Fail<LogbookCriteria>(new ExceptionBasedError(e))); }

        var count = stream.Documents.Count;
        if (count != 1)
        {
            return Task.FromResult<Result<LogbookCriteria>>(
                Result.Fail(new YamlParsingError(count == 0 ? "No YAML document found in input" : "Multiple documents found in input", EmptyPath))
            );
        }

        var document = stream.Documents.Single();

        var result = ParseCriterion(string.Empty, document.RootNode, EmptyPath);
        return Task.FromResult(result);
    }

    private IEnumerable<Result<LogbookCriteria>> ParseSubcriteria(YamlNode node,  ICollection<string> path)
    {
        if (node is not YamlMappingNode mapping)
        {
            yield return Result.Fail(new UnexpectedNodeTypeError(node.GetType(), typeof(YamlMappingNode), path));
            yield break;
        }

        foreach (var (key, value) in mapping.Children)
        {
            if (key is not YamlScalarNode scalarKey)
            {
                yield return Result.Fail(new UnexpectedNodeTypeError(key.GetType(), typeof(YamlScalarNode), path));
                continue;
            }

            var description = scalarKey.Value;
            yield return ParseCriterion(description, value, path);
        }
    }

    private Result<LogbookCriteria> ParseCriterion(string? description, YamlNode value, ICollection<string> path)
    {
        if (description is null)
        {
            return Result.Fail(new YamlParsingError("No description given for criterion", path));
        }

        var nodePath = path.Append(description).ToList();

        if (value is YamlScalarNode scalar && string.IsNullOrEmpty(scalar.Value))
        {
            return new LogbookCriteria(description, null, null, null, null, null);
        }

        if (value is not YamlMappingNode mapping)
        {
            return Result.Fail(new UnexpectedNodeTypeError(value.GetType(), typeof(YamlMappingNode), nodePath));
        }

        var subcriteria = Enumerable.Empty<Result<LogbookCriteria>>();
        if (mapping.Children.TryGetValue(SubcriteriaKey, out var subcriteriaNode))
        {
            subcriteria = ParseSubcriteria(subcriteriaNode, nodePath);
        }



        var errors = new List<IError>();
        var validCriteria = new List<LogbookCriteria>();
        foreach (var res in subcriteria)
        {
            if (res.IsSuccess)
            {
                validCriteria.Add(res.Value);
            }
            else
            {
                errors.AddRange(res.Errors);
            }
        }

        if (validCriteria.Count == 0)
        {
            validCriteria = null;
        }

        Result<LogbookCriteria> result;
        if (mapping.Children.ContainsKey(CriteriaKey))
        {
            result = ParsePredicate(description, mapping, validCriteria, path);
        }
        else if (mapping.Children.ContainsKey(TagsKey))
        {
            result = ParseTags(description, mapping, validCriteria, path);
        }
        else if (mapping.Children.ContainsKey(SubstitutionKey))
        {
            result = ParseSubstitution(description, mapping, validCriteria, path);
        }
        else
        {
            result = Result.Ok(new LogbookCriteria(description, validCriteria, null,null, null, null));
        }

        result.WithErrors(errors);
        return result;
    }

    private Result<LogbookCriteria> ParseSubstitution(string description, YamlMappingNode mapping, List<LogbookCriteria>? validCriteria, ICollection<string> path)
    {
        if (validCriteria?.Any() ?? false)
        {
            return Result.Fail(new YamlParsingError("Substitution node cannot have subcriteria", path.Append(description)));
        }

        var substitutionNodePath = path.Append(description).Append(SubstitutionKey.Value!).ToList();

        if (mapping.Children[SubstitutionKey] is not YamlScalarNode scalarNode)
        {
            return Result.Fail(new UnexpectedNodeTypeError(mapping.Children[SubstitutionKey].GetType(), typeof(YamlScalarNode), substitutionNodePath));
        }

        var substitution = parser.ParseUnaryConversion<Operation>(scalarNode.Value!);
        if (!substitution.IsSuccess)
        {
            return substitution.ToResult();
        }

        return new LogbookCriteria(description, null, null, null, substitution.Value, null);
    }

    private Result<LogbookCriteria> ParseTags(string description, YamlMappingNode mapping, List<LogbookCriteria>? validCriteria, ICollection<string> path)
    {
        var tags = ParseTagsList(mapping.Children[TagsKey], path.Append(description));
        var validTags = new List<Tag>();
        var errors = new List<IError>();

        foreach (var tag in tags)
        {
            if (tag.IsSuccess)
            {
                validTags.Add(tag.Value);
            }
            else
            {
                errors.AddRange(tag.Errors);
            }
        }

        var type = TagBasedCriterionType.Including;
        if (mapping.Children.TryGetValue(TagsModeKey, out var modeNode))
        {
            var modePath = path.Append(description).Append(TagsModeKey.Value!).ToList();
            if (modeNode is not YamlScalarNode modeScalar)
            {
                return Result.Fail(new UnexpectedNodeTypeError(modeNode.GetType(), typeof(YamlScalarNode), modePath));
            }

            if (!Enum.TryParse<TagBasedCriterionType>(modeScalar.Value, true, out var parsed))
            {
                return Result.Fail(new YamlParsingError("Unexpected tagging criterion node value given", modePath).WithMetadata("Value", modeScalar.Value!));
            }

            type = parsed;
        }

        var criterion = new LogbookCriteria(description, validCriteria, type, validTags, null, null);
        return Result.Ok(criterion).WithErrors(errors);
    }

    private IEnumerable<Result<Tag>> ParseTagsList(YamlNode node, IEnumerable<string> path)
    {
        var tagsPath = path.Append(TagsKey.Value!).ToList();
        if (node is not YamlSequenceNode seq)
        {
            yield return Result.Fail(new UnexpectedNodeTypeError(node.GetType(), typeof(YamlSequenceNode), tagsPath));
            yield break;
        }

        foreach (var tagNode in seq.Children)
        {
            if (tagNode is not YamlScalarNode tagScalar)
            {
                yield return Result.Fail(new UnexpectedNodeTypeError(tagNode.GetType(), typeof(YamlScalarNode), tagsPath));
                continue;
            }

            if (string.IsNullOrEmpty(tagScalar.Value))
            {
                yield return Result.Fail(new YamlParsingError("Empty tag value given", tagsPath));
                continue;
            }

            yield return new Tag(tagScalar.Value);
        }
    }

    private Result<LogbookCriteria> ParsePredicate(string description, YamlMappingNode mapping, List<LogbookCriteria>? subcriteria, ICollection<string> path)
    {
        var predicateNode = mapping.Children[CriteriaKey];
        var predicateNodePath = path.Append(description).Append(CriteriaKey.Value!).ToList();
        if (predicateNode is not YamlScalarNode scalarNode)
        {
            return Result.Fail(new UnexpectedNodeTypeError(predicateNode.GetType(), typeof(YamlScalarNode), predicateNodePath));
        }

        var value = scalarNode.Value;
        if (string.IsNullOrEmpty(value))
        {
            return Result.Fail(new YamlParsingError("No predicate value given", predicateNodePath));
        }
        var parseResult = parser.ParseUnaryPredicate<Operation>(value);
        if (parseResult.IsFailed)
        {
            return Result.Fail(new YamlParsingError("Failed to parse predicate value", predicateNodePath)).WithReasons(parseResult.Errors);
        }

        return new LogbookCriteria(description, subcriteria, null, null, null, parseResult.Value);
    }
}


internal class YamlParsingError(string reason, IEnumerable<string> path) : IError
{
    public string Message { get; } = reason;
    public Dictionary<string, object> Metadata { get; } = new() { {"Path", string.Join('.', path) } };
    public List<IError> Reasons { get; } = new();
}

internal class UnexpectedNodeTypeError : YamlParsingError
{
    public UnexpectedNodeTypeError(Type type, Type expected, ICollection<string> path) : base("Unexpected node type found", path)
    {
        Metadata.Add("Key", path.LastOrDefault() ?? string.Empty);
        Metadata.Add("Expected", expected.Name);
        Metadata.Add("Type", type.Name);
    }
}
