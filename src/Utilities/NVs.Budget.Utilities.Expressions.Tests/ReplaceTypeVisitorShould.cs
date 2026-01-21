using System.Linq.Expressions;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Utilities.Expressions.Tests;

public class ReplaceTypeVisitorShould
{
    private sealed class Source
    {
        public Guid Id { get; init; }
    }

    private sealed class Target
    {
        public Guid Id { get; init; }
    }

    [Fact]
    public void ConvertTypes_ShouldHandle_GenericMethodArguments()
    {
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        Expression<Func<Source, bool>> filter = s => ids.Contains(s.Id);

        var mapping = new Dictionary<Type, Type>
        {
            { typeof(Source), typeof(Target) }
        };

        var converted = filter.ConvertTypes<Source, Target>(mapping);
        var func = converted.Compile();

        Assert.True(func(new Target { Id = ids[0] }));
        Assert.False(func(new Target { Id = Guid.NewGuid() }));
    }
}
