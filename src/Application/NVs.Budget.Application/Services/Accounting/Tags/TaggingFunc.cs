﻿using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Services.Accounting.Tags;

internal record TaggingFunc(Func<TrackedOperation, Tag> Tag, Func<TrackedOperation, bool> Criterion);
