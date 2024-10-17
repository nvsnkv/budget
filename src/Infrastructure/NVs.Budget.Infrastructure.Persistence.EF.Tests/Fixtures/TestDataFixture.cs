using AutoFixture;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Domain.Entities.Accounts;
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
        var accounts = new List<TrackedBudget>();

        Owners = Fixture.Create<Generator<Owner>>().Take(2).ToList();
        foreach (var owner in Owners)
        {
            using (Fixture.SetNamedParameter(nameof(Domain.Entities.Accounts.Budget.Owners).ToLower(), new[] { owner }.AsEnumerable()))
            {
                accounts.AddRange(Fixture.Create<Generator<TrackedBudget>>().Take(2));
            }
        }

        for (var i = 0; i < accounts.Count; i++)
        {
            if (i % 2 == 1)
            {
                foreach (var owner in Owners.Except(accounts[i].Owners))
                {
                    accounts[i].AddOwner(owner);
                }
            }
        }

        Budgets = accounts;
    }

}
