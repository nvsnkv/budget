using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Domain.Entities.Budgets;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

internal record WhoamiResponse(
    bool IsAuthenticated,
    IUser? User,
    Owner? Owner
);
