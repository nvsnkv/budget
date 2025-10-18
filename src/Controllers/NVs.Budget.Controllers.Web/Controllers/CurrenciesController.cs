using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NMoneys;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class CurrenciesController : Controller
{
    /// <summary>
    /// Gets all available currencies
    /// </summary>
    /// <returns>Collection of currency codes</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<string>), 200)]
    public IReadOnlyCollection<string> GetCurrencies()
    {
        return Currency.FindAll()
            .Select(c => c.IsoCode.ToString())
            .OrderBy(c => c)
            .ToList();
    }
}

