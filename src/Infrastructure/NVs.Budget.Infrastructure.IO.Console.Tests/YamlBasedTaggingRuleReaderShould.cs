using System.Text;
using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Infrastructure.IO.Console.Input;

namespace NVs.Budget.Infrastructure.IO.Console.Tests;

public class YamlBasedTaggingRuleReaderShould
{
    private readonly Fixture _fixture = new();
    private readonly YamlBasedTaggingCriteriaReader _reader = new();

    [Fact]
    public async Task ReadValues()
    {
        var tags = _fixture.Create<Generator<string>>().Take(15).ToDictionary(x => x, x => new List<string>());
        var keys = tags.Keys.ToList();

        for (var i=0; i < tags.Keys.Count; i++)
        {
            var values = _fixture.Create<Generator<string>>().Take(i + 1);
            tags[keys[i]].AddRange(values);
        }

        var expected = tags.SelectMany(kv => kv.Value.Select(v => new TaggingCriterion(kv.Key, v))).ToList();
        var stream = GetStream(tags);

        var actual = await _reader.ReadFrom(new StreamReader(stream), CancellationToken.None).ToListAsync();

        actual.Should().AllSatisfy(r => r.Should().BeSuccess());
        actual.Select(r => r.Value).Should().BeEquivalentTo(expected);
    }

    private MemoryStream GetStream(Dictionary<string, List<string>> dict)
    {
        var builder = new StringBuilder();
        foreach (var (key, value) in dict)
        {
            builder.AppendLine($"{key}:");
            foreach (var val in value)
            {
                builder.AppendLine($"  - {val}");
            }
        }

        var text = builder.ToString();

        return new MemoryStream(Encoding.UTF8.GetBytes(text));
    }
}
