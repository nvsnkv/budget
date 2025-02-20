using System.Data;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

namespace NVs.Budget.Infrastructure.Persistence.EF.Common;

public class PostgreSqlDbMigrator(DbContext context) : IDbMigrator
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
