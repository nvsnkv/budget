using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Identity.Contracts;

public interface IIdentityService
{
    Task<IUser> GetCurrentUser(CancellationToken ct);
}
