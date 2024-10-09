using FluentResults;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.Extensions;
using NVs.Budget.Domain.ValueObjects.Criteria;
using YamlDotNet.RepresentationModel;
using Tag = NVs.Budget.Domain.ValueObjects.Tag;

namespace NVs.Budget.Infrastructure.IO.Console.Input.Criteria.Logbook;

internal class YamlLogbookRulesetReader(CriteriaParser criteriaParser, SubstitutionsParser substitutionsParser) : ILogbookCriteriaReader
{
    private static readonly string[] EmptyPath = [];
    private static readonly YamlScalarNode PredicateKey = new("predicate");
    private static readonly YamlScalarNode TagsKey = new("tags");
    private static readonly YamlScalarNode TagsModeKey = new("mode");
    private static readonly YamlScalarNode SubcriteriaKey = new("subcriteria");
    private static readonly YamlScalarNode SubstitutionKey = new("substitution");

    public Task<Result<Criterion>> ReadFrom(StreamReader reader, CancellationToken ct)
    {
        var stream = new YamlStream();
        try { stream.Load(reader); } catch(Exception e) { return Task.FromResult(Result.Fail<Criterion>(new ExceptionBasedError(e))); }

        var count = stream.Documents.Count;
        if (count != 1)
        {
            return Task.FromResult<Result<Criterion>>(
                Result.Fail(new YamlParsingError(count == 0 ? "No YAML document found in input" : "Multiple documents found in input", EmptyPath))
            );
        }

        var document = stream.Documents.Single();

        var subcriteria = ParseSubcriteria(document.RootNode, EmptyPath).ToList();
        var errors = subcriteria.Where(c => c.IsFailed).SelectMany(c => c.Errors);

        var validCriteria = subcriteria.Where(c => c.IsSuccess).ToList();
        if (validCriteria.Count == 0)
        {
            var error = new YamlParsingError("No valid criterion found!", EmptyPath);

            error.Reasons.AddRange(errors);
            error.Metadata["Key"] = string.Empty;
            var failure = Result.Fail<Criterion>(error);
            return Task.FromResult(failure);
        }

        var result = Result.Ok((Criterion)new UniversalCriterion(string.Empty, validCriteria.Select(c => c.Value)));
        result.WithErrors(errors);

        return Task.FromResult(result);
    }

    private IEnumerable<Result<Criterion>> ParseSubcriteria(YamlNode node,  ICollection<string> path)
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

    private Result<Criterion> ParseCriterion(string? description, YamlNode value, ICollection<string> path)
    {
        if (description is null)
        {
            return Result.Fail(new YamlParsingError("No description given for criterion", path));
        }

        var nodePath = path.Append(description).ToList();

        if (value is YamlScalarNode scalar && string.IsNullOrEmpty(scalar.Value))
        {
            return new UniversalCriterion(description);
        }

        if (value is not YamlMappingNode mapping)
        {
            return Result.Fail(new UnexpectedNodeTypeError(value.GetType(), typeof(YamlMappingNode), nodePath));
        }

        var subcriteria = Enumerable.Empty<Result<Criterion>>();
        if (mapping.Children.TryGetValue(SubcriteriaKey, out var subcriteriaNode))
        {
            subcriteria = ParseSubcriteria(subcriteriaNode, nodePath);
        }



        var errors = new List<IError>();
        var validCriteria = new List<Criterion>();
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

        Result<Criterion> result;
        if (mapping.Children.ContainsKey(PredicateKey))
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
            result = Result.Ok((Criterion)new UniversalCriterion(description, validCriteria));
        }

        result.WithErrors(errors);
        return result;
    }

    private Result<Criterion> ParseSubstitution(string description, YamlMappingNode mapping, List<Criterion> validCriteria, ICollection<string> path)
    {
        if (validCriteria.Any())
        {
            return Result.Fail(new YamlParsingError("Substitution node cannot have subcriteria", path.Append(description)));
        }

        var substitutionNodePath = path.Append(description).Append(SubstitutionKey.Value!).ToList();

        if (mapping.Children[SubstitutionKey] is not YamlScalarNode scalarNode)
        {
            return Result.Fail(new UnexpectedNodeTypeError(mapping.Children[SubstitutionKey].GetType(), typeof(YamlScalarNode), substitutionNodePath));
        }

        Func<Operation, string> substitution;
        try
        {
            substitution = substitutionsParser.GetSubstitutions<Operation>(scalarNode.Value!, "o");
        }
        catch (Exception e)
        {
            var error = new YamlParsingError("Failed to parse substitution", substitutionNodePath);
            error.Reasons.Add(new ExceptionBasedError(e));
            return Result.Fail(error);
        }

        return new SubstitutionBasedCriterion(description, substitution);
    }

    private Result<Criterion> ParseTags(string description, YamlMappingNode mapping, List<Criterion> validCriteria, ICollection<string> path)
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

        var criterion = new TagBasedCriterion(description, validTags, type, validCriteria);
        return Result.Ok((Criterion)criterion).WithErrors(errors);
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

    private Result<Criterion> ParsePredicate(string description, YamlMappingNode mapping, List<Criterion> subcriteria, ICollection<string> path)
    {
        var predicateNode = mapping.Children[PredicateKey];
        var predicateNodePath = path.Append(description).Append(PredicateKey.Value!).ToList();
        if (predicateNode is not YamlScalarNode scalarNode)
        {
            return Result.Fail(new UnexpectedNodeTypeError(predicateNode.GetType(), typeof(YamlScalarNode), predicateNodePath));
        }

        var value = scalarNode.Value;
        if (string.IsNullOrEmpty(value))
        {
            return Result.Fail(new YamlParsingError("No predicate value given", predicateNodePath));
        }
        var parseResult = criteriaParser.TryParsePredicate<Operation>(value, "o");
        if (parseResult.IsFailed)
        {
            return Result.Fail(new YamlParsingError("Failed to parse predicate value", predicateNodePath)).WithReasons(parseResult.Errors);
        }

        var predicate = parseResult.Value.Compile();
        return new PredicateBasedCriterion(description, predicate, subcriteria);
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
