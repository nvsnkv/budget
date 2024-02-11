﻿using NVs.Budget.Domain.Entities.Accounts;

namespace NVs.Budget.Application.Services.Storage.Accounting;

public class TrackedAccount(Guid id, string name, string bank, IEnumerable<Owner> owners)
    : Account(id, name, bank, owners), ITrackableEntity<Guid>
{
    public string? Version { get; set; }
}
