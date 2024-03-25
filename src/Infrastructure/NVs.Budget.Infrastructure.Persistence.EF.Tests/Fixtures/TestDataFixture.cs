using AutoFixture;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Infrastructure.Storage.Tests.Fixtures;

public class TestDataFixture
{
    public readonly Fixture Fixture = new();

    public IReadOnlyCollection<Owner> Owners { get; }

    public IReadOnlyCollection<TrackedAccount> Accounts { get; }

    public TestDataFixture()
    {
        var accounts = new List<TrackedAccount>();

        Owners = Fixture.Create<Generator<Owner>>().Take(2).ToList();
        foreach (var owner in Owners)
        {
            using (Fixture.SetNamedParameter(nameof(Account.Owners).ToLower(), new[] { owner }.AsEnumerable()))
            {
                accounts.AddRange(Fixture.Create<Generator<TrackedAccount>>().Take(2));
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

        Accounts = accounts;
    }

}