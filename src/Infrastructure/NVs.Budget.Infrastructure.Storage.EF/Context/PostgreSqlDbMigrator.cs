using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace NVs.Budget.Infrastructure.Storage.Context;

internal class PostgreSqlDbMigrator(BudgetContext context)
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
}
