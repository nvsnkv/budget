using AutoFixture;
using FluentAssertions;
using FluentResults.Extensions.FluentAssertions;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Persistence.EF.Repositories;
using NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class OwnersRepositoryShould : IClassFixture<DbContextManager>
{
    private readonly OwnersRepository _repo;
    private readonly Fixture _fixture;

    public OwnersRepositoryShould(DbContextManager manager)
    {
        _fixture = manager.TestData.Fixture;
        _repo = new OwnersRepository(manager.Mapper, manager.GetDbBudgetContext(), new VersionGenerator());
    }

    [Fact]
    public async Task RegisterUserAsOwner()
    {
        var user = _fixture.Create<TestUser>();
        var result = await _repo.Register(user, CancellationToken.None);

        result.Should().BeSuccess();
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Name.Should().Be(user.Name);
    }

    [Fact]
    public async Task RetrieveRegisteredOwner()
    {
        var user = _fixture.Create<TestUser>();
        var result = await _repo.Register(user, CancellationToken.None);
        result.Should().BeSuccess();
        var expected = result.Value;

        var owner = await _repo.Get(user, CancellationToken.None);
        owner.Should().NotBeNull().And.BeEquivalentTo(expected);
    }

    private class TestUser(string id, string name) : IUser
    {
        public string Id { get; } = id;

        public string Name { get; } = name;
        public Owner AsOwner()
        {
            return new Owner(Guid.Empty, Name);
        }
    }
}
