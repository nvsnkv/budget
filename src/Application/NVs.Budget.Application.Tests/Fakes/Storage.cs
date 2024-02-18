using NVs.Budget.Application.Services.Storage.Accounting;

namespace NVs.Budget.Application.Tests.Fakes;

internal class Storage
{
    public readonly FakeTransactionsRepository Transactions = new();
    public readonly FakeAccountsRepository Accounts = new();
    public readonly FakeTransfersRepository Transfers = new();
}
