using AutoFixture;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Persistence.EF.Tests.Fixtures;

public class TestDataFixture
{
    public readonly Fixture Fixture = new() { Customizations = { new ReadableExpressionsBuilder() } };

    public IReadOnlyCollection<Owner> Owners { get; }

    public IReadOnlyCollection<TrackedBudget> Budgets { get; }

    public TestDataFixture()
    {
        Fixture.Inject(LogbookCriteria.Universal);
        var budgets = new List<TrackedBudget>();

        Owners = Fixture.Create<Generator<Owner>>().Take(2).ToList();
        foreach (var owner in Owners)
        {
            using (Fixture.SetNamedParameter(nameof(Domain.Entities.Budgets.Budget.Owners).ToLower(), new[] { owner }.AsEnumerable()))
            {
                budgets.AddRange(Fixture.Create<Generator<TrackedBudget>>().Take(2));
            }
        }

        for (var i = 0; i < budgets.Count; i++)
        {
            if (i % 2 == 1)
            {
                foreach (var owner in Owners.Except(budgets[i].Owners))
                {
                    budgets[i].AddOwner(owner);
                }
            }
        }

        Budgets = budgets;
    }

}
