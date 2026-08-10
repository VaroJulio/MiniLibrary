using Microsoft.AspNetCore.Http;

namespace MiniLibrary.API.Services;

/// <summary>
/// Manages authentication cookies (access token, refresh token, CSRF token).
/// All auth tokens are stored in HttpOnly, Secure, SameSite=Strict cookies
/// to prevent XSS-based token theft.
/// </summary>
public static class CookieTokenService
{
    public const string AccessTokenCookie = "access_token";
    public const string RefreshTokenCookie = "refresh_token";
    public const string CsrfTokenCookie = "XSRF-TOKEN";
    public const string CsrfHeaderName = "X-XSRF-TOKEN";

    /// <summary>
    /// Sets authentication cookies on the response (access token, refresh token, CSRF token).
    /// </summary>
    public static void SetAuthCookies(HttpResponse response, string accessToken, string refreshToken, bool isDevelopment)
    {
        // In production with cross-origin setup (different domains for frontend/API),
        // SameSite must be None to allow cookies to be sent cross-origin.
        // This is safe because we have CSRF double-submit cookie protection.
        var sameSiteMode = isDevelopment ? SameSiteMode.Strict : SameSiteMode.None;

        var secureCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment, // Allow non-HTTPS in development
            SameSite = sameSiteMode,
            Path = "/api",
            MaxAge = TimeSpan.FromHours(1), // Match access token expiry
        };

        var refreshCookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = sameSiteMode,
            Path = "/api/auth/refresh", // Scoped to refresh endpoint only
            MaxAge = TimeSpan.FromDays(7), // Match refresh token expiry
        };

        // CSRF token cookie — NOT HttpOnly so JavaScript can read it
        var csrfToken = GenerateCsrfToken();
        var csrfCookieOptions = new CookieOptions
        {
            HttpOnly = false, // Must be readable by JavaScript
            Secure = !isDevelopment,
            SameSite = sameSiteMode,
            Path = "/",
            MaxAge = TimeSpan.FromHours(1),
        };

        response.Cookies.Append(AccessTokenCookie, accessToken, secureCookieOptions);
        response.Cookies.Append(RefreshTokenCookie, refreshToken, refreshCookieOptions);
        response.Cookies.Append(CsrfTokenCookie, csrfToken, csrfCookieOptions);

        // Also expose CSRF token as a response header for cross-origin scenarios
        // where JavaScript cannot read cookies set by a different domain.
        response.Headers.Append("X-CSRF-TOKEN", csrfToken);
    }

    /// <summary>
    /// Clears all authentication cookies from the response.
    /// </summary>
    public static void ClearAuthCookies(HttpResponse response)
    {
        var clearOptions = new CookieOptions { Path = "/api" };
        var clearRefreshOptions = new CookieOptions { Path = "/api/auth/refresh" };
        var clearCsrfOptions = new CookieOptions { Path = "/" };

        response.Cookies.Delete(AccessTokenCookie, clearOptions);
        response.Cookies.Delete(RefreshTokenCookie, clearRefreshOptions);
        response.Cookies.Delete(CsrfTokenCookie, clearCsrfOptions);
    }

    private static string GenerateCsrfToken()
    {
        var bytes = new byte[32];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }
}
