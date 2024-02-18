using System.Linq.Expressions;
using FluentAssertions;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Application.Tests;

public class ExpressionCombinationShould
{
    [Fact]
    public void CombineExpressions()
    {
        Expression<Func<Guid, bool>> left = g => g != Guid.Empty;
        var guids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        Expression<Func<Guid, bool>> right = g => guids.Contains(g);

        var expression = left.CombineWith(right);
        var func = expression.Compile();

        func(guids[0]).Should().BeTrue();
        func(Guid.Empty).Should().BeFalse();
        func(Guid.NewGuid()).Should().BeFalse();
    }
}
