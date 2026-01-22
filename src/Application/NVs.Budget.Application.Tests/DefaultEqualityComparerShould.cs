using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Domain.Entities.Budgets;

public class EqualityComparerShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void ThreatOwnersEqualIfIdIsTheSame() 
    {
        var owner = _fixture.Create<TrackedOwner>();
        var sameOwner = new TrackedOwner(owner.Id, _fixture.Create<string>());

        var comparer = EntityComparer<TrackedOwner>.Instance;
        comparer.Equals(owner,sameOwner).Should().BeTrue();
    }

    [Fact]
    public void ThreatOwnersOfDifferentTypesEqualIfIdIsTheSame() 
    {
        var trackerOwner = _fixture.Create<TrackedOwner>();
        var owner = new Owner(trackerOwner.Id, _fixture.Create<string>());

        var comparer = EntityComparer<Owner>.Instance;
        comparer.Equals(owner,trackerOwner).Should().BeTrue();
        comparer.Equals(trackerOwner,owner).Should().BeTrue();
        
    }
}