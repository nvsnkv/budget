namespace NVs.Budget.Application.Tests.Fakes;

internal class Storage
{
    public readonly FakeOperationsRepository Operations = new();
    public readonly FakeBudgetsRepository Budgets = new();
    public readonly FakeTransfersRepository Transfers = new();
}
