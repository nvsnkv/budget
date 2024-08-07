﻿using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.ValueObjects;

namespace NVs.Budget.Application.Contracts.Criteria;

public record TaggingCriterion(Func<TrackedOperation, Tag> Tag, Func<TrackedOperation, bool> Criterion);
