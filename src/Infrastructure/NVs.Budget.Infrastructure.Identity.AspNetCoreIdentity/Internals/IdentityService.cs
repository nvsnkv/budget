using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.Identity.AspNetCoreIdentity.Internals;

internal class IdentityService(IHttpContextAccessor contextAccessor, IOwnersRepository owners) : IIdentityService
{
    public async Task<IUser> GetCurrentUser(CancellationToken ct)
    {
        var id = contextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new InvalidOperationException("User is not authenticated!");
        var user = new User(id);
        var owner = await owners.Get(user, ct);
        if (owner is not null)
        {
            user.SetOwner(owner);
        }

        return user;
    }
}
