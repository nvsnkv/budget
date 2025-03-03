using System.Net;
using Asp.Versioning;
using AutoMapper;
using FluentResults;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NVs.Budget.Application.Contracts.Criteria;
using NVs.Budget.Application.Contracts.Entities.Budgeting;
using NVs.Budget.Application.Contracts.Services;
using NVs.Budget.Controllers.Web.Utils;
using NVs.Budget.Infrastructure.IO.Console.Input;

namespace NVs.Budget.Controllers.Web.Controllers;

[Authorize]
[ApiVersion("0.1")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BudgetController(
    IBudgetManager manager,
    IMapper mapper,
    ITaggingCriteriaReader tagsReader,
    ITransferCriteriaReader transfersReader,
    ILogbookCriteriaReader logbookReader
) : Controller
{
    [HttpGet]
    public async Task<IReadOnlyCollection<BudgetResponse>> GetBudgets(CancellationToken ct)
    {
        var budgets = await manager.GetOwnedBudgets(ct);
        return mapper.Map<IReadOnlyCollection<BudgetResponse>>(budgets);
    }

    [HttpGet("{id:guid}")]
    public async Task<BudgetResponse?> GetBudget(Guid id, CancellationToken ct)
    {
        var budgets = await manager.GetOwnedBudgets(ct);
        return mapper.Map<BudgetResponse?>(budgets.FirstOrDefault(b => b.Id == id));
    }

    [HttpPost,
     ProducesResponseType<BudgetResponse>(StatusCodes.Status200OK),
     ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBudget([FromBody] CreateBudgetRequest request, CancellationToken ct)
    {
        var result = await manager.Register(new(request.Name), ct);
        return result.IsSuccess ? Ok(mapper.Map<BudgetResponse>(result.Value)) : BadRequest(result.Errors);
    }

    [HttpPut,
     ProducesResponseType(StatusCodes.Status204NoContent),
     ProducesResponseType<IEnumerable<IError>>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateBudget([FromBody] UpdateBudgetRequest request, CancellationToken ct)
    {
        var budget = (await manager.GetOwnedBudgets(ct)).FirstOrDefault(b => b.Id == request.Id);
        if (budget == null)
        {
            return BadRequest("Budget not found");
        }

        var errors = new List<IError>();
        var tags = new List<TaggingCriterion>();
        var transfers = new List<TransferCriterion>();
        LogbookCriteria? logbook = null;

        using (var reader = request.TaggingCriteria.AsStreamReader())
        {
            await foreach (var tagResult in tagsReader.ReadFrom(reader, ct))
            {
                if (tagResult.IsSuccess)
                {
                    tags.Add(tagResult.Value);
                }
                else
                {
                    errors.AddRange(tagResult.Errors);
                }
            }
        }

        using (var reader = request.TransferCriteria.AsStreamReader())
        {
            await foreach (var transferResult in transfersReader.ReadFrom(reader, ct))
            {
                if (transferResult.IsSuccess)
                {
                    transfers.Add(transferResult.Value);
                }
                else
                {
                    errors.AddRange(transferResult.Errors);
                }
            }
        }

        using (var reader = request.LogbookCriteria.AsStreamReader())
        {
            var logbookResult = await logbookReader.ReadFrom(reader, ct);
            if (logbookResult.IsSuccess)
            {
                logbook = logbookResult.Value;
            }
            else
            {
                errors.AddRange(logbookResult.Errors);
            }
        }

        if (errors.Any())
        {
            return BadRequest(errors);
        }

        var result = await manager.Update(new(request.Id, request.Name, budget.Owners, tags, transfers, logbook!), ct);
        return result.IsSuccess ? NoContent() : BadRequest(result.Errors);
    }
}
