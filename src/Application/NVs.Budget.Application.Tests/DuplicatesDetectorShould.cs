using AutoFixture;
using FluentAssertions;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Services.Accounting.Duplicates;

namespace NVs.Budget.Application.Tests;

public class DuplicatesDetectorShould
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void DetectDuplicatesProperly()
    {
        var transactionA = _fixture.Create<TrackedOperation>();
        var duplicatesA = GenerateDuplicates(transactionA, TimeSpan.Zero, TimeSpan.FromDays(1), TimeSpan.FromDays(-1));
        var transactionB = _fixture.Create<TrackedOperation>();
        var duplicatesB = GenerateDuplicates(transactionB, TimeSpan.FromDays(1));
        var transactions = duplicatesA.Concat(duplicatesB).Prepend(transactionB).Append(transactionA).Concat(_fixture.Create<Generator<TrackedOperation>>().Take(5));


        var duplicates = new DuplicatesDetector(DuplicatesDetectorOptions.Default).DetectDuplicates(transactions);
        duplicates.Should().HaveCount(2);
        foreach (var duplicateList in duplicates)
        {
            duplicateList.Should().BeEquivalentTo(duplicateList.Contains(transactionA) ? duplicatesA.Append(transactionA) : duplicatesB.Prepend(transactionB));
        }
    }

    private List<TrackedOperation> GenerateDuplicates(TrackedOperation operation, params TimeSpan[] offsets) =>
        offsets.Select(offset => new TrackedOperation(
            _fixture.Create<Guid>(),
            operation.Timestamp + offset,
            operation.Amount,
            operation.Description,
            operation.Budget,
            operation.Tags,
            operation.Attributes.AsReadOnly()
        )).ToList();
}
