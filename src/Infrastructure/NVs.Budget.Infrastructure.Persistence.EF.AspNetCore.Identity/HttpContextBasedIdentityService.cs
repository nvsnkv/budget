using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.Persistence.EF.AspNetCore.Identity;

internal class HttpContextBasedIdentityService(IHttpContextAccessor httpContextAccessor, IOwnersRepository owners) : IIdentityService
{
    IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    public async Task<IUser> GetCurrentUser(CancellationToken ct)
    {
        var principal = _httpContextAccessor.HttpContext?.User ?? throw new InvalidOperationException("User not found");
        var email = principal.FindFirstValue(ClaimTypes.Email) ?? throw new InvalidOperationException("Email not found");

        var found = await owners.Get(o => o.Name == email, ct);

        return new ClaimsUser(email, found.Single());
    }
}
