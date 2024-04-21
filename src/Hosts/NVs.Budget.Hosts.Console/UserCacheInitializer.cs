using FluentResults;
using NVs.Budget.Application;
using NVs.Budget.Controllers.Console.Contracts.Errors;
using NVs.Budget.Controllers.Console.Contracts.IO.Output;

namespace NVs.Budget.Hosts.Console;

internal class UserCacheInitializer(UserCache cache, IResultWriter<Result> writer)
{
    public async Task TryInitializeCache(CancellationToken ct)
    {
        try
        {
            await cache.EnsureInitialized(ct);
        }
        catch (Exception e)
        {
            var result = Result.Fail(new ExceptionBasedError(e));
            await writer.Write(result, ct);
        }
    }
}
