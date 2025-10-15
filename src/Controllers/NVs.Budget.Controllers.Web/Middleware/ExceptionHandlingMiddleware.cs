using System.Net;
using System.Text.Json;
using FluentResults;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NVs.Budget.Controllers.Web.Exceptions;

namespace NVs.Budget.Controllers.Web.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (HttpException httpEx)
        {
            _logger.LogWarning(httpEx, "HTTP exception occurred: {Message}", httpEx.Message);
            await HandleHttpExceptionAsync(context, httpEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred: {Message}", ex.Message);
            await HandleUnhandledExceptionAsync(context, ex);
        }
    }

    private static async Task HandleHttpExceptionAsync(HttpContext context, HttpException exception)
    {
        context.Response.StatusCode = exception.StatusCode;
        context.Response.ContentType = "application/json";

        var errors = exception.Errors.Select(e => new ErrorResponse(e.Message, e.Metadata)).ToList();
        var json = JsonSerializer.Serialize(errors, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }

    private static async Task HandleUnhandledExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "application/json";

        var errors = new List<ErrorResponse>
        {
            new("An internal server error occurred. Please try again later.", new Dictionary<string, object>())
        };

        var json = JsonSerializer.Serialize(errors, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public record ErrorResponse(string Message, Dictionary<string, object> Metadata);

