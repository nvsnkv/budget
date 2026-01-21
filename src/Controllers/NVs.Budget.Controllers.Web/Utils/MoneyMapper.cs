using FluentResults;
using NMoneys;
using NVs.Budget.Controllers.Web.Models;

namespace NVs.Budget.Controllers.Web.Utils;

public class MoneyMapper
{
    public Result<Money> ParseMoney(MoneyResponse moneyResponse)
    {
        try
        {
            var currency = Currency.Get(moneyResponse.CurrencyCode);
            var money = new Money(moneyResponse.Value, currency);
            return Result.Ok(money);
        }
        catch (Exception ex)
        {
            return Result.Fail<Money>($"Invalid money value: {ex.Message}");
        }
    }
}

