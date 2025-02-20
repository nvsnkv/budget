using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Infrastructure.Persistence.EF.AspNetCore.Identity;

internal class ClaimsUser(string id, Owner owner) : IUser
{
    public string Id { get; } = id;
    public Owner AsOwner() => owner;
}
