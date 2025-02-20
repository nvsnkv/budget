using AutoMapper;
using AutoMapper.EquivalencyExpression;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Persistence.EF.Common;
using NVs.Budget.Infrastructure.Persistence.EF.Context;
using NVs.Budget.Infrastructure.Persistence.EF.Entities;
using NVs.Budget.Utilities.Expressions;
using Testcontainers.PostgreSql;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;


[CollectionDefinition(nameof(DatabaseCollectionFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DbContextManager>;

public class DbContextManager : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public readonly TestDataFixture TestData = new();
    public readonly IMapper Mapper = new Mapper(new MapperConfiguration(c =>
    {
        c.AddProfile(new MappingProfile(ReadableExpressionsParser.Default));
        c.AddCollectionMappers();
    }));

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();
        var context = GetDbBudgetContext();
        await new PostgreSqlDbMigrator(context).MigrateAsync(CancellationToken.None);

        var owners = Mapper.Map<IEnumerable<StoredOwner>>(TestData.Owners).ToList().ToDictionary(o => o.Id);
        var budgets = Mapper.Map<IEnumerable<StoredBudget>>(TestData.Budgets).ToList();

        foreach (var budget in budgets)
        {
            var storedOwners = budget.Owners.Select(o => owners[o.Id]).ToList();
            budget.Owners.Clear();
            foreach (var owner in storedOwners)
            {
                budget.Owners.Add(owner);
            }

            await context.Budgets.AddAsync(budget);
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
