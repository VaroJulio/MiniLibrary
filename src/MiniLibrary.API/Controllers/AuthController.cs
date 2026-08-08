using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MiniLibrary.API.Configuration;
using MiniLibrary.API.Extensions;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using DomainUser = MiniLibrary.Domain.Entities.User;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Handles OAuth authentication flows, JWT token generation, and token refresh.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _configuration = configuration;
        _logger = logger;
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
    /// OAuth callback handler. Provisions user on first login and returns JWT + refresh tokens.
    /// </summary>
    /// <param name="provider">OAuth provider name</param>
    /// <param name="ct">Cancellation token</param>
    /// <returns>JWT access token, refresh token, and user profile.</returns>
    /// <response code="200">Authentication successful. Returns tokens and user info.</response>
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
            return Unauthorized(new { error = "Authentication failed." });
        }

        var principal = authenticateResult.Principal;
        var externalId = principal.GetExternalId();
        var email = principal.GetEmail();
        var fullName = principal.GetFullName();

        if (string.IsNullOrEmpty(externalId))
        {
            _logger.LogWarning("External ID not found in claims for provider {Provider}", provider);
            return BadRequest(new { error = "Unable to determine user identity from provider." });
        }

        // User provisioning: check if user exists by ExternalId+Provider, create with Member role if new (Req 6.3)
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
        }

        // Generate JWT access token (60-minute expiration) and refresh token (7-day expiration) (Req 6.4)
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        _jwtTokenService.StoreRefreshToken(user.Id, refreshToken);

        // Redirect to frontend with tokens in URL (SPA OAuth flow)
        var frontendUrl = _configuration["App:FrontendUrl"] ?? "http://localhost:3000";
        var callbackUrl = $"{frontendUrl}/auth/callback?token={Uri.EscapeDataString(accessToken)}&refreshToken={Uri.EscapeDataString(refreshToken)}";
        return Redirect(callbackUrl);
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token (Req 6.5).
    /// </summary>
    /// <param name="request">The refresh token request body.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>New JWT access token and refresh token.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Refresh token not provided.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required." });
        }

        var userId = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (userId is null)
        {
            return Unauthorized(new { error = "Invalid or expired refresh token." });
        }

        var user = await _userRepository.GetByIdAsync(userId.Value, ct);
        if (user is null)
        {
            _logger.LogWarning("Refresh token valid but user {UserId} not found.", userId.Value);
            _jwtTokenService.RevokeRefreshToken(userId.Value);
            return Unauthorized(new { error = "User not found." });
        }

        // Revoke old refresh token and issue new token pair (token rotation)
        _jwtTokenService.RevokeRefreshToken(user.Id);

        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        _jwtTokenService.StoreRefreshToken(user.Id, newRefreshToken);

        return Ok(new AuthTokenResponse(
            AccessToken: newAccessToken,
            RefreshToken: newRefreshToken,
            ExpiresIn: 3600,
            User: new AuthUserResponse(
                Id: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role.ToString())));
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
    /// Logs out the current user by revoking their refresh token.
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

        return NoContent();
    }

    /// <summary>
    /// [DEV ONLY] Generates a JWT token for testing without OAuth.
    /// Enabled via Authentication:EnableDevTokens = true.
    /// </summary>
    /// <param name="request">Name, email, and role for the dev token.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns JWT access and refresh tokens.</response>
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
        }

        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        _jwtTokenService.StoreRefreshToken(user.Id, refreshToken);

        return Ok(new AuthTokenResponse(
            AccessToken: accessToken,
            RefreshToken: refreshToken,
            ExpiresIn: 3600,
            User: new AuthUserResponse(
                Id: user.Id,
                Email: user.Email,
                FullName: user.FullName,
                Role: user.Role.ToString())));
    }
}

/// <summary>
/// Response model for authentication token endpoints.
/// </summary>
public record AuthTokenResponse(
    string AccessToken,
    string RefreshToken,
    int ExpiresIn,
    AuthUserResponse User);

/// <summary>
/// User details returned in auth responses.
/// </summary>
public record AuthUserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role);

/// <summary>
/// Request model for token refresh.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);

/// <summary>
/// Request model for dev token generation.
/// </summary>
public record DevTokenRequest(string? Name = null, string? Email = null, string? Role = "Member");
