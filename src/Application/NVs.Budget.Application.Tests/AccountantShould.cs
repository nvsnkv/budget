using AutoFixture;
using Moq;
using NVs.Budget.Application.Entities.Accounting;
using NVs.Budget.Application.Entities.Contracts;
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
    private readonly TaggingCriterion _tagMeCriterion;
    private readonly TransferCriterion _exactTransferCriterion;

    private readonly Accountant _accountant;

    public AccountantShould()
    {
        _owner = _fixture.Create<Owner>();
        var user = new Mock<IUser>();
        user.Setup(u => u.AsOwner()).Returns(_owner);

        var accountManager = new AccountManager(_storage.Accounts, user.Object);

        _exactTransferCriterion = new TransferCriterion(DetectionAccuracy.Exact, "Exact transfer",
            (src, snk) => src.Amount == snk.Amount * -1
                          && src.Timestamp.Date == snk.Timestamp.Date
                          && src.Description == snk.Description);
        var transferDetector = new TransferDetector(new[]
        {
            _exactTransferCriterion
        });

        var duplicatesDetector = new DuplicatesDetector(DuplicatesDetectorSettings.Default);

        _tagMeCriterion = new TaggingCriterion(new("TagMe!"), t => t.Description == "Tag me!");
        var tagsManager = new TagsManager(new[] { _tagMeCriterion });

        _accountant = new(
            _storage.Operations,
            _storage.Transfers,
            accountManager,
            tagsManager,
            new TransfersListBuilder(transferDetector),
            new ImportResultBuilder(duplicatesDetector)
        );
    }

    [Fact]
    public async Task ImportIncomingTransactions()
    {
        _fixture.SetNamedParameter("owners", Enumerable.Repeat(_owner, 1));
        var account = _fixture.Create<TrackedAccount>();
        _storage.Accounts.Append([account]);
        _fixture.ResetNamedParameter<IEnumerable<Owner>>("owners");

        var data = new ImportTestData(_fixture, [account], _owner);

        var result = await _accountant.ImportTransactions(data.Operations, new ImportOptions(true, DetectionAccuracy.Exact), CancellationToken.None);

        data.VerifyResult(result);
    }
}
