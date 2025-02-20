using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Persistence.EF.Context;

public interface IDbMigrator
 {
    Task MigrateAsync(CancellationToken ct);
}
