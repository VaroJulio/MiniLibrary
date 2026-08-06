using System.Security.Claims;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.API.Extensions;

/// <summary>
/// Extension methods for extracting user information from ClaimsPrincipal.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    /// <summary>
    /// Gets the internal user ID from the "sub" or custom "userId" claim.
    /// </summary>
    public static Guid? GetUserId(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst("userId") ?? principal.FindFirst(ClaimTypes.NameIdentifier);
        if (claim is not null && Guid.TryParse(claim.Value, out var id))
        {
            return id;
        }
        return null;
    }

    /// <summary>
    /// Gets the user's role from the "role" claim.
    /// </summary>
    public static UserRole? GetRole(this ClaimsPrincipal principal)
    {
        var claim = principal.FindFirst(ClaimTypes.Role) ?? principal.FindFirst("role");
        if (claim is not null && Enum.TryParse<UserRole>(claim.Value, ignoreCase: true, out var role))
        {
            return role;
        }
        return null;
    }

    /// <summary>
    /// Gets the user's email from claims.
    /// </summary>
    public static string? GetEmail(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Email)?.Value
            ?? principal.FindFirst("email")?.Value;
    }

    /// <summary>
    /// Gets the user's full name from claims.
    /// </summary>
    public static string? GetFullName(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.Name)?.Value
            ?? principal.FindFirst("name")?.Value;
    }

    /// <summary>
    /// Gets the external identity provider ID (subject from OAuth).
    /// </summary>
    public static string? GetExternalId(this ClaimsPrincipal principal)
    {
        return principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? principal.FindFirst("sub")?.Value;
    }

    /// <summary>
    /// Gets the authentication provider name (e.g., "Google", "Microsoft").
    /// </summary>
    public static string? GetProvider(this ClaimsPrincipal principal)
    {
        return principal.Identity?.AuthenticationType;
    }
}
