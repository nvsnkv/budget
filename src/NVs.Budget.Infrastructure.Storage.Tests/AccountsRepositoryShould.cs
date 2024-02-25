using AutoFixture;
using AutoMapper;
using FluentAssertions;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Storage.Entities;
using NVs.Budget.Infrastructure.Storage.Repositories;
using NVs.Budget.Infrastructure.Storage.Tests.Fixtures;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Storage.Tests;

[Collection(nameof(DatabaseCollectionFixture))]
public class AccountsRepositoryShould
{
    private readonly Fixture _fixture = new();
    private readonly DatabaseCollectionFixture.PostgreSqlDbContext _contextAccessor;
    private readonly AccountsRepository _repo;
    private readonly IMapper _mapper;

    public AccountsRepositoryShould(DatabaseCollectionFixture.PostgreSqlDbContext contextAccessor)
    {
        _contextAccessor = contextAccessor;

        _mapper = new Mapper(new MapperConfiguration(c => c.AddProfile(new MappingProfile())));
        _repo = new AccountsRepository(_mapper, _contextAccessor.GetDbBudgetContext());
    }

    [Fact]
    public async Task RegisterAnAccount()
    {
        var owner = _mapper.Map<Owner>(await GetOwner());

        var account = _fixture.Create<UnregisteredAccount>();
        var result = await _repo.Register(account, owner, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var created = result.Value;
        created.Should().NotBeNull();
        created.Should().BeEquivalentTo(account);

        var red = await _repo.Get(a => a.Id == created.Id, CancellationToken.None);
        red.Should().HaveCount(1);
        red.Single().Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task UpdateAccount()
    {
        var id = _fixture.Create<Guid>();
        _fixture.SetNamedParameter(nameof(id), id);
        var account = _fixture.Build<StoredAccount>()
            .Without(a => a.Owners)
            .Without(a => a.Transactions)
            .Create();

        var owner = await GetOwner();
        account.Owners.Add(owner);

        await using var context = _contextAccessor.GetDbBudgetContext();
        await context.Accounts.AddAsync(account);
        await context.SaveChangesAsync();

        var expected = _fixture.Create<TrackedAccount>();
        expected.AddOwner(_mapper.Map<Owner>(owner));

        var updated = await _repo.Update(expected, CancellationToken.None);
        updated.IsSuccess.Should().BeTrue();
        updated.Value.Should().BeEquivalentTo(expected);
    }



    private async Task<StoredOwner> GetOwner()
    {
        var owner = _mapper.Map<StoredOwner>(_fixture.Create<Owner>());
        await using var context = _contextAccessor.GetDbBudgetContext();
        await context.AddAsync(owner);
        await context.SaveChangesAsync();
        return owner;
    }
}
