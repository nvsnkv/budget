using AutoFixture;
using FluentAssertions;
using NVs.Budget.Domain.Entities.Operations;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class SubstitutionBasedCriterionShould
{
    [Fact]
    public void GenerateSubcriteriaForEachSubstitution()
    {
        Func<Operation,string> substitution = o => $"Year {o.Timestamp.Year}";
        var criterion = new SubstitutionBasedCriterion("subst", substitution);
        var operations = new Fixture().Create<Generator<Operation>>().Take(15).ToList();

        foreach (var operation in operations)
        {
            criterion.Matched(operation).Should().BeTrue();
            var sub = criterion.GetMatchedSubcriterion(operation);
            sub.Should().NotBeNull();
            sub!.Description.Should().Be(substitution(operation));
            sub!.Matched(operation).Should().BeTrue();
        }

        var expectedDescriptions = operations.Select(o => substitution(o)).Distinct();
        criterion.Subcriteria.Select(c => c.Description).Should().BeEquivalentTo(expectedDescriptions);
    }

    [Fact]
    public void NotDuplicateSubcriteria()
    {
        var operation = new Fixture().Create<Operation>();
        var expectedDescription = "TEXT";
        var criterion = new SubstitutionBasedCriterion("subst", o => expectedDescription);

        criterion.GetMatchedSubcriterion(operation);
        criterion.GetMatchedSubcriterion(operation);
        criterion.GetMatchedSubcriterion(operation);

        criterion.Subcriteria.Should().HaveCount(1);
        criterion.Subcriteria.Single().Description.Should().Be(expectedDescription);

    }
}
