using AutoFixture;
using Moq;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities;
using NVs.Budget.Application.Contracts.Entities.Accounting;
using NVs.Budget.Application.Contracts.Options;
using NVs.Budget.Application.Services.Accounting;
using NVs.Budget.Application.Services.Accounting.Duplicates;
using NVs.Budget.Application.Services.Accounting.Results;
using NVs.Budget.Application.Services.Accounting.Tags;
using NVs.Budget.Application.Services.Accounting.Transfers;
using NVs.Budget.Application.Tests.Fakes;
using NVs.Budget.Domain.Entities.Accounts;
using NVs.Budget.Utilities.Testing;

namespace NVs.Budget.Application.Tests;

public class AccountantShould
{
    private readonly Fixture _fixture = new();
    private readonly Storage _storage = new();

    private readonly Owner _owner;

    private readonly Accountant _accountant;

    public AccountantShould()
    {
        _owner = _fixture.Create<Owner>();
        var user = new Mock<IUser>();
        user.Setup(u => u.AsOwner()).Returns(_owner);

        var budgetManager = new BudgetManager(_storage.Budgets, user.Object);

        var exactTransferCriterion = new TransferCriterion(DetectionAccuracy.Exact, "Exact transfer",
            (src, snk) => src.Amount == snk.Amount * -1
                          && src.Timestamp.Date == snk.Timestamp.Date
                          && src.Description == snk.Description);
        var transferDetector = new TransferDetector(new[]
        {
            exactTransferCriterion
        });

        var duplicatesDetector = new DuplicatesDetector(DuplicatesDetectorOptions.Default);

        _accountant = new(
            _storage.Operations,
            _storage.Transfers,
            budgetManager,
            new TransfersListBuilder(transferDetector),
            new ImportResultBuilder(duplicatesDetector)
        );
    }

    [Fact]
    public async Task ImportIncomingTransactions()
    {
        _fixture.SetNamedParameter("owners", Enumerable.Repeat(_owner, 1));
        _fixture.SetNamedParameter("taggingCriteria", Enumerable.Empty<TaggingCriterion>());
        var budget = _fixture.Create<TrackedBudget>();
        _storage.Budgets.Append([budget]);
        _fixture.ResetNamedParameter<IEnumerable<Owner>>("owners");

        var data = new ImportTestData(_fixture, [budget]);

        var result = await _accountant.ImportOperations(data.Operations, budget, new ImportOptions(DetectionAccuracy.Exact), CancellationToken.None);

        data.VerifyResult(result);
    }
}
