using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace NVs.Budget.Controllers.Web.Filters;

/// <summary>
/// Action filter that validates model state and returns 400 Bad Request with errors
/// if the model state is invalid (e.g., from input formatter failures)
/// </summary>
internal class ValidateModelStateFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            var errors = context.ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .SelectMany(x => x.Value!.Errors.Select(e => new 
                { 
                    Field = x.Key, 
                    Message = string.IsNullOrEmpty(e.ErrorMessage) ? e.Exception?.Message : e.ErrorMessage 
                }))
                .ToList();

            context.Result = new BadRequestObjectResult(errors);
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {
        // No action needed after execution
    }
}

