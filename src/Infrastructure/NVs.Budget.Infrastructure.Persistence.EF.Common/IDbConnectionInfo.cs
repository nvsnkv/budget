using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Persistence.EF.Common;

public interface IDbConnectionInfo
{
    public string? DataSource { get; }
    public string? Database { get; }
    public string? Context { get; }
}
