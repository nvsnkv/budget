using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace NVs.Budget.Infrastructure.Persistence.EF.Context;

internal class PostgreSqlDbMigrator(BudgetContext context) : IDisposable, IAsyncDisposable, IDbMigrator
{
    public async Task MigrateAsync(CancellationToken ct)
    {
        await context.Database.MigrateAsync(ct);

        if (context.Database.GetDbConnection() is NpgsqlConnection npgsqlConnection)
        {
            if (npgsqlConnection.State != ConnectionState.Open)
            {
                await npgsqlConnection.OpenAsync(ct);
            }
            try
            {
                await npgsqlConnection.ReloadTypesAsync();
            }
            finally
            {
                await npgsqlConnection.CloseAsync();
            }
        }
    }

    public void Dispose()
    {
        context.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        await context.DisposeAsync();
    }
}

public interface IDbMigrator : IAsyncDisposable
{
    Task MigrateAsync(CancellationToken ct);
}
