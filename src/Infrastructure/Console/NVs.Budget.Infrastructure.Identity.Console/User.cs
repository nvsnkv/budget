using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Infrastructure.Identity.Console;

internal class User : IUser
{
    public string Id { get; init; } = string.Empty;
    public Owner? Owner { get; set; }
    public Owner AsOwner() => Owner ?? new Owner(Owner.Invalid.Id, Id);
}
