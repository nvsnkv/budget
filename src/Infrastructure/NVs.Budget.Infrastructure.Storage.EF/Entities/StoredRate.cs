using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;
using NMoneys;

namespace NVs.Budget.Infrastructure.Storage.Entities;

internal class StoredRate(DateTime asOf, CurrencyIsoCode from, CurrencyIsoCode to, decimal rate) : DbRecord
{
    [Key, UsedImplicitly]
    public Guid Id { get; init; } = Guid.Empty;
    public DateTime AsOf { get; private set; } = asOf;
    public  CurrencyIsoCode From { get; private set; } = from;
    public  CurrencyIsoCode To { get; private set; } = to;
    public  decimal Rate { get; private set; } = rate;

    public virtual StoredOwner Owner { get; init; } = StoredOwner.Invalid;
}
