using System.Text.Json.Serialization;
using NMoneys;

namespace NVs.Budget.Infrastructure.Persistence.EF.Entities;

internal class StoredPlanExpectation
{
    public Guid Id { get; init; }

    public decimal ExpectedAmount { get; init; }

    public CurrencyIsoCode CurrencyCode { get; init; }

    public DateTime From { get; init; }

    public DateTime Till { get; init; }

    /// <summary>Hierarchical criterion path (same convention as logbook).</summary>
    [JsonPropertyName("subcriterionPath")]
    public string? SubcriterionPath { get; init; }

    /// <summary>Legacy JSON key; persisted historical rows may only populate this.</summary>
    [JsonPropertyName("subcriterionName")]
    public string? SubcriterionName { get; init; }

    public string? Note { get; init; }

    /// <summary>Effective path after deserialization from JSON.</summary>
    [JsonIgnore]
    public string? ResolvedCriterionPath =>
        string.IsNullOrWhiteSpace(SubcriterionPath) ? SubcriterionName?.Trim() : SubcriterionPath.Trim();
}
