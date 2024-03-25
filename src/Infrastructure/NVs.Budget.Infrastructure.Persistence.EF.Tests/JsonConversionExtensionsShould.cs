using AutoFixture;
using FluentAssertions;
using NVs.Budget.Infrastructure.Persistence.EF.Context.DictionariesSupport;

namespace NVs.Budget.Infrastructure.Storage.Tests;

public class JsonConversionExtensionsShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ConvertDictionaryToStringAndBack()
    {
        var expected = new Dictionary<string, object>();
        expected[_fixture.Create<string>()] = _fixture.Create<string>();
        expected[_fixture.Create<string>()] = _fixture.Create<decimal>();
        expected[_fixture.Create<string>()] = true;
        expected[_fixture.Create<string>()] = false;
        expected[_fixture.Create<string>()] = new Dictionary<string, object>()
        {
            { _fixture.Create<string>(), _fixture.Create<string>() }
        };

        var str = expected.ToJsonString();
        var actual = str.ToDictionary();

        actual.Should().BeEquivalentTo(expected);
    }
}
