using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Persistence.EF.Context;

internal class DbConnectionInfo : IDbConnectionInfo
{
    public DbConnectionInfo(BudgetContext? context)
    {
        if (context is null)
        {
            DataSource = Database = "Not set!";
            return;
        }

        var connection = context.Database.GetDbConnection();
        DataSource = connection.DataSource;
        Database = connection.Database;
    }

    public string? DataSource { get; }
    public string? Database { get; }
}