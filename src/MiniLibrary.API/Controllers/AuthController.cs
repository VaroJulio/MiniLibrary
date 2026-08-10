using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MiniLibrary.API.Configuration;
using MiniLibrary.API.Extensions;
using MiniLibrary.API.Services;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using DomainUser = MiniLibrary.Domain.Entities.User;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Handles OAuth authentication flows, JWT token generation via HttpOnly cookies, and token refresh.
/// Tokens are never exposed to JavaScript — they are stored in HttpOnly, Secure, SameSite=Strict cookies.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;
    private readonly IWebHostEnvironment _environment;
    private readonly IUnitOfWork _unitOfWork;

    public AuthController(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<AuthController> logger,
        IWebHostEnvironment environment,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
        _environment = environment;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Initiates OAuth login flow with the specified provider.
    /// </summary>
    /// <param name="provider">OAuth provider: "Google" or "Microsoft"</param>
    /// <returns>Challenge redirect to the external OAuth provider.</returns>
    /// <response code="302">Redirects to the OAuth provider's login page.</response>
    /// <response code="400">Unsupported provider specified.</response>
    [HttpGet("login/{provider}")]
    [AllowAnonymous]
    public IActionResult Login(string provider)
    {
        // Normalize provider name to match registered scheme (e.g., "google" → "Google")
        var normalizedProvider = provider.ToLower() switch
        {
            "google" => "Google",
            "microsoft" => "Microsoft",
            _ => provider
        };

        if (normalizedProvider != "Google" && normalizedProvider != "Microsoft")
        {
            return BadRequest(new { error = "Unsupported authentication provider. Use 'Google' or 'Microsoft'." });
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback), new { provider = normalizedProvider })
        };

        return Challenge(properties, normalizedProvider);
    }

    /// <summary>
    /// OAuth callback handler. Provisions user on first login, sets auth cookies, and redirects to frontend.
    /// Tokens are set as HttpOnly cookies — never exposed in URLs or response bodies.
    /// </summary>
    /// <param name="provider">OAuth provider name</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="302">Sets auth cookies and redirects to frontend.</response>
    /// <response code="400">Unable to determine user identity from provider.</response>
    /// <response code="401">Authentication with external provider failed.</response>
    [HttpGet("callback/{provider}")]
    [AllowAnonymous]
    public async Task<IActionResult> Callback(string provider, CancellationToken ct)
    {
        var authenticateResult = await HttpContext.AuthenticateAsync(provider);
        if (!authenticateResult.Succeeded || authenticateResult.Principal is null)
        {
            _logger.LogWarning("Authentication failed for provider {Provider}", provider);
            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/login?error=auth_failed");
        }

        var principal = authenticateResult.Principal;
        var externalId = principal.GetExternalId();
        var email = principal.GetEmail();
        var fullName = principal.GetFullName();

        if (string.IsNullOrEmpty(externalId))
        {
            _logger.LogWarning("External ID not found in claims for provider {Provider}", provider);
            var frontendUrl = GetFrontendUrl();
            return Redirect($"{frontendUrl}/login?error=no_identity");
        }

        // User provisioning: check if user exists by ExternalId+Provider, create with Member role if new
        var user = await _userRepository.GetByExternalIdAsync(externalId, provider, ct);

        if (user is null)
        {
            _logger.LogInformation(
                "First-time login for user {Email} via {Provider}. Creating account with Member role.",
                email, provider);

            user = DomainUser.Create(
                email: email ?? string.Empty,
                fullName: fullName ?? string.Empty,
                externalId: externalId,
                provider: provider,
                role: UserRole.Member);

            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.CommitAsync(ct);
        }

        // Generate tokens and set as HttpOnly cookies
        SetAuthCookiesForUser(user);

        // Redirect to frontend — no tokens in URL
        var redirectUrl = GetFrontendUrl();
        return Redirect($"{redirectUrl}/auth/callback");
    }

    /// <summary>
    /// Refreshes an expired access token using the refresh token cookie.
    /// Sets new auth cookies (token rotation).
    /// </summary>
    /// <response code="200">Tokens refreshed. New cookies set.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        // Read refresh token from cookie (not from request body)
        var refreshToken = Request.Cookies[CookieTokenService.RefreshTokenCookie];

        if (string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(new { error = "No refresh token." });
        }

        var userId = _jwtTokenService.ValidateRefreshToken(refreshToken);
        if (userId is null)
        {
            CookieTokenService.ClearAuthCookies(Response);
            return Unauthorized(new { error = "Invalid or expired refresh token." });
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, ct);
        if (user is null)
        {
            _logger.LogWarning("Refresh token valid but user {UserId} not found.", userId.Value);
            _jwtTokenService.RevokeRefreshToken(userId.Value);
            CookieTokenService.ClearAuthCookies(Response);
            return Unauthorized(new { error = "User not found." });
        }

        // Revoke old refresh token and issue new token pair (token rotation)
        _jwtTokenService.RevokeRefreshToken(user.Id);
        SetAuthCookiesForUser(user);

        return Ok(new AuthUserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString()));
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    /// <returns>The authenticated user's profile information.</returns>
    /// <response code="200">Returns the user profile.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User not found in the database.</response>
    [HttpGet("me")]
    [Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
    public async Task<IActionResult> GetCurrentUser(CancellationToken ct)
    {
        var userId = User.GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, ct);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(new AuthUserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString()));
    }

    /// <summary>
    /// Logs out the current user by revoking refresh token and clearing auth cookies.
    /// </summary>
    [HttpPost("logout")]
    [Authorize]
    public IActionResult Logout()
    {
        var userId = User.GetUserId();
        if (userId is not null)
        {
            _jwtTokenService.RevokeRefreshToken(userId.Value);
        }

        CookieTokenService.ClearAuthCookies(Response);
        return NoContent();
    }

    /// <summary>
    /// [DEV ONLY] Generates auth cookies for testing without OAuth.
    /// Enabled via Authentication:EnableDevTokens = true.
    /// </summary>
    /// <param name="request">Name, email, and role for the dev token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Auth cookies set. Returns user info.</response>
    /// <response code="404">Dev tokens are disabled.</response>
    [HttpPost("dev-token")]
    [AllowAnonymous]
    public async Task<IActionResult> DevToken([FromBody] DevTokenRequest request, CancellationToken ct)
    {
        var enabled = _configuration.GetValue<bool>("Authentication:EnableDevTokens");
        if (!enabled)
        {
            return NotFound(new { error = "Dev tokens are disabled. Set Authentication:EnableDevTokens = true." });
        }

        var role = request.Role ?? "Member";
        if (role != "Admin" && role != "Librarian" && role != "Member")
        {
            return BadRequest(new { error = "Role must be Admin, Librarian, or Member." });
        }

        var email = request.Email ?? $"dev-{role.ToLower()}@minilibrary.local";
        var name = request.Name ?? $"Dev {role}";

        // Find or create the dev user
        var externalId = $"dev-{email}";
        var user = await _userRepository.GetByExternalIdAsync(externalId, "DevToken", ct);

        if (user is null)
        {
            var parsedRole = Enum.Parse<UserRole>(role);
            user = DomainUser.Create(email, name, externalId, "DevToken", parsedRole);
            await _userRepository.AddAsync(user, ct);
            await _unitOfWork.CommitAsync(ct);
        }

        // Set auth cookies (tokens never returned in response body)
        SetAuthCookiesForUser(user);

        return Ok(new AuthUserResponse(
            Id: user.Id,
            Email: user.Email,
            FullName: user.FullName,
            Role: user.Role.ToString()));
    }

    /// <summary>
    /// Helper: generates access + refresh tokens and sets them as HttpOnly cookies.
    /// </summary>
    private void SetAuthCookiesForUser(DomainUser user)
    {
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        _jwtTokenService.StoreRefreshToken(user.Id, refreshToken);

        CookieTokenService.SetAuthCookies(Response, accessToken, refreshToken, _environment.IsDevelopment());
    }

    /// <summary>
    /// Gets the primary frontend URL (first entry if comma-separated list).
    /// </summary>
    private string GetFrontendUrl()
    {
        var configured = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
        // Support comma-separated list — use first entry for redirects
        var firstUrl = configured.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).FirstOrDefault();
        return firstUrl?.TrimEnd('/') ?? "http://localhost:3000";
    }
}

/// <summary>
/// User details returned in auth responses.
/// </summary>
public record AuthUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role);

/// <summary>
/// Request model for dev token generation.
/// </summary>
public record DevTokenRequest(string? Name = null, string? Email = null, string? Role = "Member");
