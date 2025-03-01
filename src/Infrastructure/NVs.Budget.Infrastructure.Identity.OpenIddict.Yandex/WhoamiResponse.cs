using NVs.Budget.Application.Contracts.Entities;

namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex;

internal record WhoamiResponse(
    bool IsAuthenticated,
    IUser? User
);
