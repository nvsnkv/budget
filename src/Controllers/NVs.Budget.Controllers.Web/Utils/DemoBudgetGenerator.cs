using NMoneys;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;
using NVs.Budget.Utilities.Expressions;

namespace NVs.Budget.Controllers.Web.Utils;

public interface IDemoBudgetGenerator
{
    DemoBudgetSeed Generate(Guid budgetId, DateTime nowUtc);
}

public record DemoBudgetSeed(
    IReadOnlyCollection<TaggingCriterion> TaggingCriteria,
    IReadOnlyCollection<LogbookCriteria> LogbookCriteria,
    IReadOnlyCollection<UnregisteredOperation> Operations
);

internal class DemoBudgetGenerator(ReadableExpressionsParser parser) : IDemoBudgetGenerator
{
    private static readonly string[] Categories = ["food", "transport", "shopping", "utilities", "health", "entertainment"];
    private static readonly Currency Currency = Currency.Get("RUB");

    public DemoBudgetSeed Generate(Guid budgetId, DateTime nowUtc)
    {
        var random = new Random(budgetId.GetHashCode());
        var count = random.Next(200, 501);
        var from = nowUtc.AddMonths(-6);

        var taggingCriteria = new[]
        {
            CreateTaggingCriterion("o => \"Withdraws\"", "o => o.Amount.Amount < 0"),
            CreateTaggingCriterion("o => \"Expenses\"", "o => o.Amount.Amount < 0 && !o.Description.Contains(\"Transfer\")")
        };

        var byCategorySubstitution = parser
            .ParseUnaryConversion<Operation>("o => o.Attributes.ContainsKey(\"category\") ? Convert.ToString(o.Attributes[\"category\"]) : \"uncategorized\"")
            .Value;

        var logbookCriteria = new[]
        {
            new LogbookCriteria(
                "Demo",
                [
                    new LogbookCriteria(
                        "Withdraws",
                        [
                            new LogbookCriteria("By category", null, null, null, byCategorySubstitution, null, null)
                        ],
                        TagBasedCriterionType.Including,
                        [new Tag("Withdraws")],
                        null,
                        null,
                        null
                    ),
                    new LogbookCriteria(
                        "Expenses",
                        [
                            new LogbookCriteria("By category", null, null, null, byCategorySubstitution, null, null)
                        ],
                        TagBasedCriterionType.Including,
                        [new Tag("Expenses")],
                        null,
                        null,
                        null
                    )
                ],
                null,
                null,
                null,
                null,
                true
            )
        };

        var operations = Enumerable.Range(0, count).Select(_ => CreateOperation(random, from, nowUtc)).ToList();

        return new DemoBudgetSeed(taggingCriteria, logbookCriteria, operations);
    }

    private TaggingCriterion CreateTaggingCriterion(string tagExpression, string conditionExpression)
    {
        return new TaggingCriterion(
            parser.ParseUnaryConversion<TrackedOperation>(tagExpression).Value,
            parser.ParseUnaryPredicate<TrackedOperation>(conditionExpression).Value
        );
    }

    private static UnregisteredOperation CreateOperation(Random random, DateTime from, DateTime till)
    {
        var timestamp = from.AddSeconds(random.NextDouble() * (till - from).TotalSeconds);
        var isExpense = random.NextDouble() < 0.85d;

        var amount = isExpense
            ? -Math.Round((decimal)(50 + random.NextDouble() * 4950), 2)
            : Math.Round((decimal)(15000 + random.NextDouble() * 165000), 2);

        if (isExpense)
        {
            var category = Categories[random.Next(Categories.Length)];
            var attributes = new Dictionary<string, object>
            {
                ["category"] = category,
                ["source"] = "demo"
            };

            return new UnregisteredOperation(
                timestamp,
                new Money(amount, Currency),
                $"Demo {category} expense",
                attributes
            );
        }

        return new UnregisteredOperation(
            timestamp,
            new Money(amount, Currency),
            "Demo income",
            new Dictionary<string, object> { ["source"] = "demo" }
        );
    }
}
