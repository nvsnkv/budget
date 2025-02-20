using Microsoft.Extensions.Logging;
using NVs.Budget.Application;

namespace NVs.Budget.Hosts.Console;

internal class UserCacheInitializer(UserCache cache, ILogger<UserCacheInitializer> logger)
{
    public async Task TryInitializeCache(CancellationToken ct)
    {
        using var _ = logger.BeginScope("[User cache init]");
        {

            logger.LogDebug("Initializing user cache...");
            try
            {
                await cache.EnsureInitialized(ct);
                logger.LogDebug("Cache initialized");
            }
            catch (Exception e)
            {
                logger.LogWarning(e, "Failed to initialize cache! Most of the operations would not work properly!");
            }
        }
    }
}
