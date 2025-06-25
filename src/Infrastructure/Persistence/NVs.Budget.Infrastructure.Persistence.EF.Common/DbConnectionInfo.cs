using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Persistence.EF.Common;

public class DbConnectionInfo : IDbConnectionInfo
{
    public DbConnectionInfo(DbContext? context)
    {
        if (context is null)
        {
            DataSource = Database = "Not set!";
            return;
        }

        var connection = context.Database.GetDbConnection();
        DataSource = connection.DataSource;
        Database = connection.Database;
        Context = context.GetType().Name;
    }

    public string? DataSource { get; }
    public string? Database { get; }
    public string? Context { get; }
}
