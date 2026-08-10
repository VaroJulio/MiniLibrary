using MiniLibrary.API.Services;

namespace MiniLibrary.API.Middleware;

/// <summary>
/// CSRF protection using the double-submit cookie pattern.
/// For state-changing requests (POST, PUT, DELETE, PATCH), validates that the
/// X-XSRF-TOKEN header matches the XSRF-TOKEN cookie value.
/// 
/// This works because:
/// - The XSRF-TOKEN cookie is set by the server (SameSite=Strict prevents cross-site sending)
/// - JavaScript on our origin reads the cookie and sends it as a header
/// - An attacker on a different origin cannot read our cookies, so cannot forge the header
/// </summary>
public class CsrfProtectionMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> SafeMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        "GET", "HEAD", "OPTIONS"
    };

    public CsrfProtectionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip CSRF validation for safe methods
        if (SafeMethods.Contains(context.Request.Method))
        {
            await _next(context);
            return;
        }

        // Skip CSRF validation for unauthenticated requests (login endpoints)
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        // Skip CSRF validation when using Bearer token auth (not cookie-based)
        // CSRF protection is only needed for cookie-based auth since browsers auto-send cookies
        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        // Validate CSRF token: header must match cookie
        var cookieToken = context.Request.Cookies[CookieTokenService.CsrfTokenCookie];
        var headerToken = context.Request.Headers[CookieTokenService.CsrfHeaderName].FirstOrDefault();

        if (string.IsNullOrEmpty(cookieToken) || string.IsNullOrEmpty(headerToken))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "CSRF validation failed",
                status = 403,
                detail = "Missing CSRF token. Include X-XSRF-TOKEN header with value from XSRF-TOKEN cookie."
            });
            return;
        }

        if (!string.Equals(cookieToken, headerToken, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "CSRF validation failed",
                status = 403,
                detail = "CSRF token mismatch."
            });
            return;
        }

        await _next(context);
    }
}
