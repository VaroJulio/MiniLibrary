using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace MiniLibrary.API.Configuration;

/// <summary>
/// Custom authorization middleware result handler that returns RFC 7807 ProblemDetails
/// with a message indicating the required permission when authorization fails.
/// Implements Requirement 6.6: "retornar código HTTP 403 con un mensaje indicando el permiso requerido"
/// </summary>
public class AuthorizationResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // If authorization succeeded, continue the pipeline
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        // If the user is not authenticated at all → 401
        if (authorizeResult.Challenged)
        {
            var problemDetails401 = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Unauthorized",
                Status = StatusCodes.Status401Unauthorized,
                Detail = "Authentication is required to access this resource.",
                Instance = context.Request.Path
            };

            var correlationId = context.Items["CorrelationId"]?.ToString();
            if (correlationId is not null)
            {
                problemDetails401.Extensions["correlationId"] = correlationId;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/problem+json";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsJsonAsync(problemDetails401, options);
            return;
        }

        // User is authenticated but lacks permissions → 403
        if (authorizeResult.Forbidden)
        {
            var requiredPermission = GetRequiredPermissionMessage(policy, context);

            var problemDetails403 = new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7807",
                Title = "Forbidden",
                Status = StatusCodes.Status403Forbidden,
                Detail = requiredPermission,
                Instance = context.Request.Path
            };

            var correlationId = context.Items["CorrelationId"]?.ToString();
            if (correlationId is not null)
            {
                problemDetails403.Extensions["correlationId"] = correlationId;
            }

            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            context.Response.ContentType = "application/problem+json";

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            await context.Response.WriteAsJsonAsync(problemDetails403, options);
            return;
        }

        // Fallback to default handler for any other case
        await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
    }

    /// <summary>
    /// Determines the required permission message based on the authorization policy.
    /// Maps policies to human-readable permission descriptions.
    /// </summary>
    private static string GetRequiredPermissionMessage(AuthorizationPolicy policy, HttpContext context)
    {
        // Check policy requirements to determine what's needed
        foreach (var requirement in policy.Requirements)
        {
            if (requirement is Microsoft.AspNetCore.Authorization.Infrastructure.ClaimsAuthorizationRequirement claimsReq)
            {
                var allowedValues = claimsReq.AllowedValues?.ToList();
                if (allowedValues is not null && allowedValues.Count > 0)
                {
                    if (allowedValues.Contains("Admin"))
                    {
                        return "This resource requires the Admin role. Contact your system administrator.";
                    }
                }
            }
        }

        // Check assertion-based policies by inspecting what the user lacks
        var userRole = context.User.FindFirst("role")?.Value
            ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            ?? "unknown";

        var path = context.Request.Path.Value ?? "";

        // Determine required permission based on path patterns
        if (path.Contains("/users", StringComparison.OrdinalIgnoreCase))
        {
            return "Access denied. User management requires the Admin role.";
        }

        if (path.Contains("/dashboard", StringComparison.OrdinalIgnoreCase))
        {
            return "Access denied. Dashboard access requires the Librarian or Admin role.";
        }

        if (path.Contains("/books", StringComparison.OrdinalIgnoreCase) &&
            (context.Request.Method == "POST" || context.Request.Method == "PUT" || context.Request.Method == "DELETE"))
        {
            return "Access denied. Book management requires the Librarian or Admin role.";
        }

        return $"You do not have sufficient permissions to access this resource. Current role: {userRole}.";
    }
}
