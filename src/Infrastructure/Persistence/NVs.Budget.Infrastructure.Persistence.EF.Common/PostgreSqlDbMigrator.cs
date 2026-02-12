using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql;
using NVs.Budget.Infrastructure.Persistence.EF.Context;

namespace NVs.Budget.Infrastructure.Persistence.EF.Common;

public class PostgreSqlDbMigrator<T>(T context) : IDbMigrator where T : DbContext
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

    public async Task<MigrationRollbackResult> RollbackLastMigrationAsync(CancellationToken ct)
    {
        var appliedMigrations = (await context.Database.GetAppliedMigrationsAsync(ct)).ToList();
        if (appliedMigrations.Count == 0)
        {
            return new MigrationRollbackResult(typeof(T).Name, null, null, false);
        }

        var fromMigration = appliedMigrations[^1];
        var toMigration = appliedMigrations.Count > 1
            ? appliedMigrations[^2]
            : "0";

        var migrator = context.Database.GetService<IMigrator>();
        await migrator.MigrateAsync(toMigration, ct);
        return new MigrationRollbackResult(typeof(T).Name, fromMigration, toMigration, true);
    }
}
