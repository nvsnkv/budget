using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace NVs.Budget.Infrastructure.Persistence.EF.Common;

public class DesignTimeContextFactory<T> : IDesignTimeDbContextFactory<T> where T: DbContext
{
    public T CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<T>().UseNpgsql().Options;
        return (T)(Activator.CreateInstance(typeof(T), options) ?? throw new InvalidOperationException($"Could not create instance of type {typeof(T).Name}"));
    }
}
