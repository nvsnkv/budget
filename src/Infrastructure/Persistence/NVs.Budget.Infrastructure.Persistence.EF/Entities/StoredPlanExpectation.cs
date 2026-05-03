using NMoneys;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredPlanExpectation
{
    public Guid Id { get; init; }

    public decimal ExpectedAmount { get; init; }

    public CurrencyIsoCode CurrencyCode { get; init; }

    public DateTime From { get; init; }

    public DateTime Till { get; init; }

    public string? SubcriterionName { get; init; }

    public string? Note { get; init; }
}
