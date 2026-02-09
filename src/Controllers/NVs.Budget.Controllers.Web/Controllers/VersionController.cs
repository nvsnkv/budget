using System.Reflection;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace NVs.Budget.Controllers.Web.Controllers;

[AllowAnonymous]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class VersionController : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(VersionResponse), 200)]
    public VersionResponse GetVersion()
    {
        var entryAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = entryAssembly.GetName().Version?.ToString() ?? "unknown";
        return new VersionResponse(version);
    }
}

public record VersionResponse(string Version);
