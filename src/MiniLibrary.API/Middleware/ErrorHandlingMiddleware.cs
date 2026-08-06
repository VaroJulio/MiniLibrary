using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Common.Exceptions;
using ValidationException = MiniLibrary.Application.Common.Exceptions.ValidationException;

namespace MiniLibrary.API.Middleware;

/// <summary>
/// Global exception handling middleware that maps exceptions to RFC 7807 ProblemDetails responses.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? "unknown";

        var (statusCode, title, detail, errors) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status422UnprocessableEntity,
                "Validation Error",
                "One or more validation errors occurred.",
                validationEx.Errors),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundEx.Message,
                (IDictionary<string, string[]>?)null),

            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictEx.Message,
                (IDictionary<string, string[]>?)null),

            UnauthorizedAccessException unauthorizedEx => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                unauthorizedEx.Message,
                (IDictionary<string, string[]>?)null),

            DbUpdateConcurrencyException => (
                StatusCodes.Status409Conflict,
                "Conflict",
                "The resource was modified by another request. Please retry.",
                (IDictionary<string, string[]>?)null),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred.",
                (IDictionary<string, string[]>?)null)
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(
                exception,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}",
                correlationId);
        }
        else
        {
            _logger.LogWarning(
                exception,
                "Handled exception: {ExceptionType}. CorrelationId: {CorrelationId}",
                exception.GetType().Name,
                correlationId);
        }

        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = context.Request.Path
        };

        problemDetails.Extensions["correlationId"] = correlationId;

        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsJsonAsync(problemDetails, options);
    }
}
