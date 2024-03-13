using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;
using Testcontainers.PostgreSql;

namespace NVs.Budget.Infrastructure.Storage.Tests.Fixtures;


[CollectionDefinition(nameof(DatabaseCollectionFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DbContextManager>;

public class DbContextManager : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public readonly TestDataFixture TestData = new();
    public readonly IMapper Mapper = new Mapper(new MapperConfiguration(c =>
    {
        c.AddProfile<MappingProfile>();
        c.AddCollectionMappers();
    }));

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        var context = GetDbBudgetContext();
        await context.Database.MigrateAsync();

        var owners = Mapper.Map<IEnumerable<StoredOwner>>(TestData.Owners).ToList().ToDictionary(o => o.Id);
        var accounts = Mapper.Map<IEnumerable<StoredAccount>>(TestData.Accounts).ToList();

        foreach (var account in accounts)
        {
            var storedOwners = account.Owners.Select(o => owners[o.Id]).ToList();
            account.Owners.Clear();
            foreach (var owner in storedOwners)
            {
                account.Owners.Add(owner);
            }

            await context.Accounts.AddAsync(account);
        }

        await context.SaveChangesAsync();
    }

    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }

    internal BudgetContext GetDbBudgetContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<BudgetContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString() + ";Include Error Detail=true")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        return new BudgetContext(optionsBuilder.Options);
    }
}
