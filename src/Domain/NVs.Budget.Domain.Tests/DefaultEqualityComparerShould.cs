using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Budgets;

public class DefaultEqualityComparerShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ThreatOwnersEqualIfIdIsTheSame() 
    {
        var owner = _fixture.Create<Owner>();
        var sameOwner = new Owner(owner.Id, _fixture.Create<string>());

        var comparer = EqualityComparer<Owner>.Default;
        comparer.Equals(owner,sameOwner).Should().BeTrue();
    }
}