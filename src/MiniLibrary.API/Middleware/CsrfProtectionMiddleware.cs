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

        // In cross-origin deployments, the browser sends the XSRF-TOKEN cookie
        // but JavaScript cannot read third-party cookies via document.cookie.
        // We use a defense-in-depth approach:
        // 1. If the X-XSRF-TOKEN header is present → validate double-submit (standard)
        // 2. If the header is missing but request has a custom header (X-Correlation-Id)
        //    → the request must have passed CORS preflight, which already validates origin.
        //    A cross-site attacker cannot set custom headers without CORS permission.
        var cookieToken = context.Request.Cookies[CookieTokenService.CsrfTokenCookie];
        var headerToken = context.Request.Headers[CookieTokenService.CsrfHeaderName].FirstOrDefault();

        // Standard double-submit validation when both are present
        if (!string.IsNullOrEmpty(cookieToken) && !string.IsNullOrEmpty(headerToken))
        {
            // Cookie values may be URL-encoded by the browser; decode for comparison
            var decodedCookieToken = Uri.UnescapeDataString(cookieToken);
            if (!string.Equals(decodedCookieToken, headerToken, StringComparison.Ordinal))
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
            return;
        }

        // Cross-origin fallback: if the request has a custom header (proving it passed
        // CORS preflight and originates from an allowed origin), accept it.
        // This is safe because: browsers enforce CORS preflight for custom headers,
        // and our CORS policy only allows specific origins.
        var hasCustomHeader = !string.IsNullOrEmpty(
            context.Request.Headers["X-Correlation-Id"].FirstOrDefault());
        var hasOriginHeader = !string.IsNullOrEmpty(
            context.Request.Headers.Origin.FirstOrDefault());

        if (hasCustomHeader && hasOriginHeader)
        {
            await _next(context);
            return;
        }

        // No valid CSRF proof found
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
            title = "CSRF validation failed",
            status = 403,
            detail = "Missing CSRF token. Include X-XSRF-TOKEN header with value from XSRF-TOKEN cookie."
        });
    }
}
