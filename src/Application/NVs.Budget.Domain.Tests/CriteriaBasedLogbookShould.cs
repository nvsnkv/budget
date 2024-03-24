using FluentAssertions;
using NVs.Budget.Domain.Aggregates;
using NVs.Budget.Domain.Errors;
using NVs.Budget.Domain.ValueObjects;
using NVs.Budget.Domain.ValueObjects.Criteria;

namespace NVs.Budget.Domain.Tests;

public class CriteriaBasedLogbookShould : LogbookShould
{
    [Fact]
    public void RegisterValidTransactionsForItselfAndForChildren()
    {
        var oddTag = new Tag("Odd");
        var evenTag = new Tag("Even");
        var oddCriteria = new TagBasedCriterion("Odds", new[] { oddTag }, TagBasedCriterionType.Including);
        var evenCriteria = new TagBasedCriterion("Evens", new[] { evenTag }, TagBasedCriterionType.Including);
        var criterion = new UniversalCriterion("Universal", new[] { oddCriteria,evenCriteria });

        var odds = TestData.GenerateTestTransactions(30, oddTag);
        var evens = TestData.GenerateTestTransactions(20, evenTag);

        var book = new CriteriaBasedLogbook(criterion);
        var results = odds.Concat(evens).Select(t => book.Register(t)).ToList();

        results.Select(r => r.IsSuccess).Should().AllBeEquivalentTo(true);

        book.Children.Should().HaveCount(criterion.Subcriteria.Count);
        book.Children[oddCriteria].Operations.Should().BeEquivalentTo(odds.OrderBy(t => t.Timestamp));
        book.Children[evenCriteria].Operations.Should().BeEquivalentTo(evens.OrderBy(t => t.Timestamp));
    }

    [Fact]
    public void CreateSubRangedLogbooksWithSameCriteria()
    {
        var now = DateTime.Now;
        var pastCriterion = new PredicateBasedCriterion("past", t => t.Timestamp < now);
        var futureCriterion = new UniversalCriterion("future");
        var criterion = new UniversalCriterion("Universal", new[]
        {
            pastCriterion,
            futureCriterion
        });

        var transactions = TestData.GenerateTestTransactions(100);
        var book = new CriteriaBasedLogbook(criterion);
        var results = transactions.Select(t => book.Register(t)).ToList();

        results.Select(r => r.IsSuccess).Should().AllBeEquivalentTo(true);

        var subRanged = book[now, DateTime.MaxValue];

        subRanged.Should().BeOfType<CriteriaBasedLogbook>();
        var logBook = (CriteriaBasedLogbook)subRanged;

        logBook.Criterion.Should().Be(criterion);
        logBook.Children[pastCriterion].Operations.Should().BeEmpty();

        var expectedFutureTransactions = transactions.Where(t => t.Timestamp >= now).OrderBy(t => t.Timestamp);
        logBook.Children[futureCriterion].Operations.Should().BeEquivalentTo(expectedFutureTransactions);
    }

    [Fact]
    public void NotAddTransactionIfItDoesNotMatchCriterion()
    {
        var transaction = TestData.GenerateTestTransactions(1).First();
        var criterion = new PredicateBasedCriterion("Not this one!", t => t != transaction);

        var logbook = new CriteriaBasedLogbook(criterion);
        var result = logbook.Register(transaction);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<OperationDidNotMatchCriteriaError>();
    }

    [Fact]
    public void NotAddTransactionIfItDoesNotMatchSubcriteria()
    {
        var transaction = TestData.GenerateTestTransactions(1).First();
        var criterion = new PredicateBasedCriterion("Not this one!", t => t != transaction);

        var logbook = new CriteriaBasedLogbook(new UniversalCriterion("Anyone", new []{ criterion }));
        var result = logbook.Register(transaction);

        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().HaveCount(1);
        result.Errors.Single().Should().BeOfType<OperationDidNotMatchSubcriteriaError>();
    }

    protected override Logbook CreateLogbook() => new CriteriaBasedLogbook(new UniversalCriterion("Anyone"));
}
