using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Budgets;

public class OwnerShould 
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void BeEqualToOtherOwnerWithTheSameId() 
    {
        var owner = _fixture.Create<Owner>();
        var sameOwner = new Owner(owner.Id, _fixture.Create<string>());

        owner.GetHashCode().Should().Be(sameOwner.GetHashCode());
        owner.Equals(sameOwner).Should().BeTrue();
        (owner == sameOwner).Should().BeTrue();
    }
}
