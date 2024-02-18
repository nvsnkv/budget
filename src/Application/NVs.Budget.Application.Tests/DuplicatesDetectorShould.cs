using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Services.Accounting.Duplicates;

namespace NVs.Budget.Application.Tests;

public class DuplicatesDetectorShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void DetectDuplicatesProperly()
    {
        var transactionA = _fixture.Create<TrackedTransaction>();
        var duplicatesA = GenerateDuplicates(transactionA, TimeSpan.Zero, TimeSpan.FromDays(1), TimeSpan.FromDays(-1));
        var transactionB = _fixture.Create<TrackedTransaction>();
        var duplicatesB = GenerateDuplicates(transactionB, TimeSpan.FromDays(1));
        var transactions = duplicatesA.Concat(duplicatesB).Prepend(transactionB).Append(transactionA).Concat(_fixture.Create<Generator<TrackedTransaction>>().Take(5));


        var duplicates = new DuplicatesDetector(DuplicatesDetectorSettings.Default).DetectDuplicates(transactions);
        duplicates.Should().HaveCount(2);
        foreach (var duplicateList in duplicates)
        {
            duplicateList.Should().BeEquivalentTo(duplicateList.Contains(transactionA) ? duplicatesA.Append(transactionA) : duplicatesB.Prepend(transactionB));
        }
    }

    private List<TrackedTransaction> GenerateDuplicates(TrackedTransaction transaction, params TimeSpan[] offsets) =>
        offsets.Select(offset => new TrackedTransaction(
            _fixture.Create<Guid>(),
            transaction.Timestamp + offset,
            transaction.Amount,
            transaction.Description,
            transaction.Account,
            transaction.Tags,
            transaction.Attributes.AsReadOnly()
        )).ToList();
}
