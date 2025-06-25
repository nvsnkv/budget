using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Infrastructure.Identity.Contracts;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Infrastructure.Identity.Console;

internal class EnvironmentBasedIdentityService(IOwnersRepository owners) : IIdentityService
{
    public async Task<IUser> GetCurrentUser(CancellationToken ct)
    {
        var uname = Environment.UserName;
        var user = new User
        {
            Id = uname,
        };

        user.Owner = await owners.Get(user, ct);

        return user;
    }
}
