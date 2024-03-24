namespace NVs.Budget.Application.Tests.Fakes;

internal class Storage
{
    public readonly FakeOperationsRepository Operations = new();
    public readonly FakeAccountsRepository Accounts = new();
    public readonly FakeTransfersRepository Transfers = new();
}
