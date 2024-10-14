using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Budgeting;

namespace NVs.Budget.Infrastructure.IO.Console.Tests.TestData.FileWithDotsInNumbersAndCyrillicAttributes;

internal class FileWithDotsInNumbersAndCyrillicAttributes
{
    public static readonly List<UnregisteredOperation> Operations = new()
    {
        new UnregisteredOperation(DateTime.Parse("2023-10-07 16:10:19+00"), new Money(-5000, CurrencyIsoCode.RUB), "Детский Мир",
            new Dictionary<string, object>
            {
                { "Category", "Детские товары" }
            })
    };
}
