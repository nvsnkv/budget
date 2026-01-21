using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Domain.Entities.Budgets;
using NVs.Budget.Infrastructure.Persistence.Contracts.Accounting;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OwnersController(IOwnersRepository owners) : Controller
{
    [HttpGet]
    public async Task<IReadOnlyCollection<Owner>> Get(CancellationToken ct) => await owners.Get(o => true, ct);
}
