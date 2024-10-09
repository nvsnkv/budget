using NMoneys;
using NVs.Budget.Application.Contracts.Entities.Accounting;

namespace NVs.Budget.Controllers.Console.IO.Tests.TestData;

internal class ValidFile
{
    public static readonly List<UnregisteredOperation> Operations = new()
    {
        new UnregisteredOperation(DateTime.Parse("30.10.2023"), new Money(-11, CurrencyIsoCode.RUB), "MOSKVA\\OZON RU",
            new Dictionary<string, object>()
            {
                { "MCC", "5300" },
                { "Category", "Супермаркеты" }
            },
            new UnregisteredAccount("Счет")),
        new UnregisteredOperation(DateTime.Parse("23.10.2023"), new Money(32, CurrencyIsoCode.RUB), "Зарплатка",
            new Dictionary<string, object>()
            {
                { "MCC", "" },
                { "Category", "Зарплата" }
            },
            new UnregisteredAccount("Еще счет")),
        new UnregisteredOperation(DateTime.Parse("22.10.2023"), new Money(-1515.2m , CurrencyIsoCode.RUB), "Moscow\\YANDEX 5814 EDA",
            new Dictionary<string, object>()
            {
                { "MCC", "3990" },
                { "Category", "Фастфуд" }
            },
            new UnregisteredAccount("Счет"))
    };
}
