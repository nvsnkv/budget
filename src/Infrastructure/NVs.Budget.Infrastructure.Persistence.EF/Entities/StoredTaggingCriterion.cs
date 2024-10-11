using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredTaggingCriterion
{
    public string Tag { get; set; } = string.Empty;
    public string Condition { get; set; } = string.Empty;

    public virtual StoredBudget Budget { get; set; } = StoredBudget.Invalid;
}
