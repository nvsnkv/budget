namespace NVs.Budget.Infrastructure.Identity.OpenIddict.Yandex.Mapping;

public class UserToOwnerMapping
{
    public required Guid OwnerId { get; init; }
    public required string UserId { get; init; }
}
