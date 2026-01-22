using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;

public class TrackedOwnerShould 
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void BeEqualToOtherOwnerWithTheSameId() 
    {
        var owner = _fixture.Create<TrackedOwner>();
        var sameOwner = new TrackedOwner(owner.Id, _fixture.Create<string>());

        owner.GetHashCode().Should().Be(sameOwner.GetHashCode());
        owner.Equals(sameOwner).Should().BeTrue();
        (owner == sameOwner).Should().BeTrue();
    }
}
