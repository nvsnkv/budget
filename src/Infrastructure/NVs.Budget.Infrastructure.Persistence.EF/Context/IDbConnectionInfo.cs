namespace NVs.Budget.Infrastructure.Persistence.EF.Context;

public interface IDbConnectionInfo
{
    public string? DataSource { get; }

    public string? Database { get; }
}