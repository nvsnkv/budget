using Asp.Versioning;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Web.Models;
using NVs.Budget.Infrastructure.IO.Console.Input;
using NVs.Budget.Infrastructure.IO.Console.Options;
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
    ICsvReadingOptionsReader csvOptionsReader,
    IBudgetSpecificSettingsRepository settingsRepository
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
            var yamlContent = await streamReader.ReadToEndAsync();
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
    public async Task<IActionResult> UpdateCsvReadingOptions(
        Guid id,
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
            // Find existing budget
            var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == id);
            if (budget == null)
            {
                return BadRequest("Budget not found");
            }

            // Read and parse options
            using var streamReader = new StreamReader(file.OpenReadStream());
            var result = await csvOptionsReader.ReadFrom(streamReader, ct);
            if (result.IsFailed)
            {
                return BadRequest(result.Errors);
            }

            // Update options
            var updateResult = await settingsRepository.UpdateReadingOptionsFor(budget, result.Value, ct);
            return updateResult.IsSuccess ? NoContent() : BadRequest(updateResult.Errors);
        }
        catch (Exception ex)
        {
            return BadRequest(Result.Fail(ex.Message).Errors);
        }
    }

    [HttpGet("{id:guid}/csv-options.yaml")]
    [Produces("application/yaml")]
    [ProducesResponseType<CsvReadingOptions>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CsvReadingOptions>> GetCsvReadingOptions(Guid id, CancellationToken ct)
    {
        // Find existing budget
        var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == id);
        if (budget == null)
        {
            return NotFound("Budget not found");
        }

        // Get and return options - will be automatically serialized to YAML
        return await settingsRepository.GetReadingOptionsFor(budget, ct);
    }
}