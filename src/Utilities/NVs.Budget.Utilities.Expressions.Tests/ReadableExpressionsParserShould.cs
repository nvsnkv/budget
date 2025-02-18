using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;

namespace NVs.Budget.Utilities.Expressions.Tests;

public class ReadableExpressionsParserShould
{
    private readonly ReadableExpressionsParser _parser = new();

    [Theory]
    [InlineData("o => o.Equals(2)", "", false)]
    [InlineData("(o) => o.Equals(2)", "", false)]
    [InlineData("( o ) => o.Equals(2)", 2, true)]
    [InlineData("(o)=>o.Equals(2)", 2, true)]
    [InlineData("o=>o.Equals(2)", 2, true)]
    public void ParseUnaryPredicate(string predicate, object value, bool expected)
    {
        var expr = _parser.ParseUnaryPredicate<object>(predicate);
        expr.Should().BeSuccess();

        Func<object, bool> testFn = expr.Value;

        testFn(value).Should().Be(expected);
    }


    [Theory]
    [InlineData("(l,r) => l.Value == r.Value", "a", "b", false)]
    [InlineData("(someCrazyName_1,SomeCrazyName2) => someCrazyName_1.Value == SomeCrazyName2.Value", "a", "a", true)]
    [InlineData("(l, r) => l.Value == r.Value", "a", "b", false)]
    [InlineData("( l , r ) => l.Value == r.Value", "a", "b", false)]
    public void ParseBinaryPredicate(string predicate, string leftVal, string rightVal, bool expected)
    {
        var left = new TestRecord(leftVal);
        var right = new TestRecord(rightVal);

        var expr = _parser.ParseBinaryPredicate<TestRecord, TestRecord>(predicate);
        expr.Should().BeSuccess();

        Func<TestRecord, TestRecord,  bool> testFn = expr.Value;

        testFn(left, right).Should().Be(expected);
    }

    [Theory]
    [InlineData("o => o.ToString()", "!", "!")]
    [InlineData("(o) => o.ToString()", "", "")]
    [InlineData("( o ) => o.ToString()", "2", "2")]
    public void ParseUnaryConverion(string predicate, object value, string expected)
    {
        var expr = _parser.ParseUnaryConversion<object>(predicate);
        expr.Should().BeSuccess();

        Func<object, string> testFn = expr.Value;

        testFn(value).Should().Be(expected);
    }

    [Fact]
    public void ParseMultilineBinaryPredicate()
    {
        var predicate = @"(l, r) => l == l
                    && r != r";

        var expr = _parser.ParseBinaryPredicate<object, object>(predicate);
        expr.Should().BeSuccess();
        Func<object, object, bool> testFn = expr.Value;
        testFn(new object(), new object()).Should().BeFalse();
    }

    public record TestRecord(string Value);
}
