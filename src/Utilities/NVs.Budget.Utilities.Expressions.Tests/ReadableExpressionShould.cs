using System.Linq.Expressions;
using FluentAssertions;

namespace NVs.Budget.Utilities.Expressions.Tests;

public class ReadableExpressionShould
{
    private readonly string _representation = "o => o.GetHashCode() % 2 == 0";
    private readonly Expression<Func<object,bool>> _expression = o => o.GetHashCode() % 2 == 0;

    [Fact]
    public void BeImplicitlyCastableToExpression()
    {
        var readable = new ReadableExpression<Func<object, bool>>(_representation, _expression);
        Expression<Func<object, bool>> testExpression = readable;

        testExpression.Should().Be(_expression);
    }

    [Fact]
    public void BeImplictlyCastableToFunc()
    {
        var readable = new ReadableExpression<Func<object, bool>>(_representation, _expression);
        Func<object, bool> testFn = readable;

        var o = new object();
        var expectedValue = _expression.Compile()(o);
        testFn(o).Should().Be(expectedValue);
    }

    [Fact]
    public void ReturnItsRepresentationFromToStringMethod()
    {
        var readable = new ReadableExpression<Func<object, bool>>(_representation, _expression);
        readable.ToString().Should().Be(_representation);
    }
}
