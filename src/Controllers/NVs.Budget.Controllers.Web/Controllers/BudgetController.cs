using Asp.Versioning;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Infrastructure.Files.CSV.Contracts;
using NVs.Budget.Utilities.Expressions;
using YamlDotNet.Serialization;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BudgetController(
    IBudgetManager manager,
    IMapper mapper,
    ReadableExpressionsParser parser,
    IDeserializer yamlDeserializer,
    IReadingSettingsRepository  readingSettingsRepository,
    ILogger<BudgetController> logger
) : Controller
{
    [HttpGet]
    public async Task<IReadOnlyCollection<BudgetConfiguration>> GetBudgets(CancellationToken ct)
    {
        var budgets = await manager.GetOwnedBudgets(ct);
        return mapper.Map<IReadOnlyCollection<BudgetConfiguration>>(budgets);
    }

    [HttpGet("{id:guid}")]
    public async Task<BudgetConfiguration?> GetBudget(Guid id, CancellationToken ct)
    {
        var budgets = await manager.GetOwnedBudgets(ct);
        return mapper.Map<BudgetConfiguration?>(budgets.FirstOrDefault(b => b.Id == id));
    }

    [HttpGet("{id:guid}.yaml")]
    [Produces("application/yaml")]
    public async Task<BudgetConfiguration?> GetBudgetConfiguration(Guid id, CancellationToken ct)
    {
        var budgets = await manager.GetOwnedBudgets(ct);
        return mapper.Map<BudgetConfiguration?>(budgets.FirstOrDefault(b => b.Id == id));
    }

    [HttpPost,
     ProducesResponseType<BudgetConfiguration>(StatusCodes.Status200OK),
     ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request, CancellationToken ct)
    {
        var result = await manager.Register(new(request.Name), ct);
        return result.IsSuccess ? Ok(mapper.Map<BudgetConfiguration>(result.Value)) : BadRequest(result.Errors);
    }

    [HttpPut,
     ProducesResponseType(StatusCodes.Status204NoContent),
     ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBudget([FromBody] BudgetConfiguration request, CancellationToken ct)
    {
        var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == request.Id);
        if (budget == null)
        {
            return BadRequest("Budget not found");
        }

        var result = budget.Patch(request, parser);

        if (result.IsFailed)
        {
            return BadRequest(result.Errors);
        }

        result = await manager.Update(result.Value, ct);

        return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
    }
    
    [HttpPut(":upload")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBudgetFromYaml(
        IFormFile file, 
        CancellationToken ct)
    {
        // Validate file
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded");
        }

        if (!file.FileName.EndsWith(".yaml") && !file.FileName.EndsWith(".yml"))
        {
            return BadRequest("Only YAML files are supported");
        }

        try
        {
            // Read and parse YAML
            using var streamReader = new StreamReader(file.OpenReadStream());
            var yamlContent = await streamReader.ReadToEndAsync(ct);
            var request = yamlDeserializer.Deserialize<BudgetConfiguration>(yamlContent);

            // Find existing budget
            var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == request.Id);
            if (budget == null)
            {
                return BadRequest("Budget not found");
            }

            // Apply changes
            var result = budget.Patch(request, parser);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            // Update budget
            result = await manager.Update(result.Value, ct);
            return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail(ex.Message).Errors);
        }
    }

    [HttpPut("{id:guid}/csv-options")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateCsvReadingOptions(
        Guid id,
        IFormFile file,
        CancellationToken ct)
    {
        // Validate file
        if (file.Length == 0)
        {
            return BadRequest(Enumerable.Repeat(new Error("No file uploaded"), 1));
        }

        if (!file.FileName.EndsWith(".yaml") && !file.FileName.EndsWith(".yml"))
        {
            return BadRequest(Enumerable.Repeat(new Error("Only YAML files are supported"), 1));
        }

        try
        {
            // Find existing budget
            var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == id);
            if (budget == null)
            {
                return BadRequest(Enumerable.Repeat(new Error("Budget not found"), 1));
            }

            // Read and parse options
            using var streamReader = new StreamReader(file.OpenReadStream());
            var yamlContent = await streamReader.ReadToEndAsync(ct);
            var request = yamlDeserializer.Deserialize<IDictionary<string, CsvFileReadingConfiguration>>(yamlContent);

            var settings = request.ConvertToSettings();
            if (settings.IsFailed)
            {
                return BadRequest(settings.Errors);
            }

            var result = await readingSettingsRepository.UpdateReadingSettingsFor(budget, settings.Value, ct);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            return NoContent();
        }
        catch (YamlDotNet.Core.YamlException ex) 
        {
            logger.LogError(ex, "Error parsing YAML file");
            logger.LogError(ex.InnerException, "Inner exception:");
            return BadRequest(Enumerable.Repeat(new ExceptionalError(ex.ToString(), ex), 1));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating CSV reading options");
            return StatusCode(StatusCodes.Status500InternalServerError, Enumerable.Repeat(new ExceptionalError(ex.ToString(), ex), 1));
        }
    }

    [HttpGet("{id:guid}/csv-options.yaml")]
    [Produces("application/yaml")]
    [ProducesResponseType<IDictionary<string, CsvFileReadingConfiguration>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IDictionary<string, CsvFileReadingConfiguration>>> GetCsvReadingOptions(Guid id, CancellationToken ct)
    {
        // Find existing budget
        var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == id);
        if (budget == null)
        {
            return NotFound("Budget not found");
        }

        // Get and return options - will be automatically serialized to YAML
        var options = await readingSettingsRepository.GetReadingSettingsFor(budget, ct);
        return Ok(mapper.Map<IDictionary<string, CsvFileReadingConfiguration>>(options));
    }
}
