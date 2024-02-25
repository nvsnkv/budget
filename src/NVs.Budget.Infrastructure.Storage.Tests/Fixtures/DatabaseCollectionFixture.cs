using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Infrastructure.Storage.Context;
using NVs.Budget.Infrastructure.Storage.Entities;
using Testcontainers.PostgreSql;

namespace NVs.Budget.Infrastructure.Storage.Tests.Fixtures;


[CollectionDefinition(nameof(DatabaseCollectionFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DatabaseCollectionFixture.PostgreSqlDbContext>
{
    public class PostgreSqlDbContext : IAsyncLifetime
    {
        private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

        public async Task InitializeAsync()
        {
            await _postgreSqlContainer.StartAsync();
            var context = GetDbBudgetContext();
            await context.Database.MigrateAsync();
        }

        public Task DisposeAsync()
        {
            return _postgreSqlContainer.DisposeAsync().AsTask();
        }

        internal BudgetContext GetDbBudgetContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<BudgetContext>()
                .UseNpgsql(_postgreSqlContainer.GetConnectionString());

            return new BudgetContext(optionsBuilder.Options);
        }
    }
}
