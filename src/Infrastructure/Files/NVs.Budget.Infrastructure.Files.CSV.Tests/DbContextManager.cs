using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using NVs.Budget.Infrastructure.Files.CSV.Persistence.Settings;
using Testcontainers.PostgreSql;

namespace NVs.Budget.Infrastructure.Files.CSV.Tests;

[CollectionDefinition(nameof(DatabaseCollectionFixture))]
public class DatabaseCollectionFixture : ICollectionFixture<DbContextManager>;

[UsedImplicitly]
public class DbContextManager : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder().Build();

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        var context = GetSettingsContext();
        await context.Database.MigrateAsync();
    }

    internal SettingsContext GetSettingsContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<SettingsContext>()
            .UseNpgsql(_postgreSqlContainer.GetConnectionString() + ";Include Error Detail=true")
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();

        var context =  new SettingsContext(optionsBuilder.Options);
        return context;
    }


    public Task DisposeAsync()
    {
        return _postgreSqlContainer.DisposeAsync().AsTask();
    }
}
