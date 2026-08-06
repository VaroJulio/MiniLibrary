using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MiniLibrary.API.Extensions;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using DomainUser = MiniLibrary.Domain.Entities.User;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Handles OAuth authentication flows, JWT token generation, and user provisioning.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IUserRepository userRepository,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Initiates OAuth login flow with the specified provider.
    /// </summary>
    /// <param name="provider">OAuth provider: "Google" or "Microsoft"</param>
    [HttpGet("login/{provider}")]
    [AllowAnonymous]
    public IActionResult Login(string provider)
    {
        if (!string.Equals(provider, "Google", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(provider, "Microsoft", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { error = "Unsupported authentication provider. Use 'Google' or 'Microsoft'." });
        }

        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action(nameof(Callback), new { provider })
        };

        return Challenge(properties, provider);
    }

    /// <summary>
    /// OAuth callback handler. Provisions user on first login and returns JWT tokens.
    /// </summary>
    /// <param name="provider">OAuth provider name</param>
    /// <param name="ct">Cancellation token</param>
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

        // User provisioning: check if user exists by ExternalId+Provider, create with Member role if new
        var user = await _userRepository.GetByExternalIdAsync(externalId, provider, ct);

        if (user is null)
        {
            _logger.LogInformation("First-time login for user {Email} via {Provider}. Creating account with Member role.",
                email, provider);

            user = DomainUser.Create(
                email: email ?? string.Empty,
                fullName: fullName ?? string.Empty,
                externalId: externalId,
                provider: provider,
                role: UserRole.Member);

            await _userRepository.AddAsync(user, ct);
        }

        // Generate JWT token with user's role claim
        var accessToken = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        return Ok(new
        {
            accessToken,
            refreshToken,
            expiresIn = 3600, // 60 minutes in seconds
            user = new
            {
                id = user.Id,
                email = user.Email,
                fullName = user.FullName,
                role = user.Role.ToString()
            }
        });
    }

    /// <summary>
    /// Refreshes an expired JWT token using a valid refresh token.
    /// </summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public IActionResult Refresh([FromBody] RefreshTokenRequest request)
    {
        // In a full implementation, validate the refresh token against stored tokens.
        // For now, this endpoint documents the contract.
        if (string.IsNullOrEmpty(request.RefreshToken))
        {
            return BadRequest(new { error = "Refresh token is required." });
        }

        // TODO: Validate refresh token against stored tokens and issue new JWT
        return Unauthorized(new { error = "Invalid or expired refresh token." });
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
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

        return Ok(new
        {
            id = user.Id,
            email = user.Email,
            fullName = user.FullName,
            role = user.Role.ToString(),
            createdAt = user.CreatedAt
        });
    }

    private string GenerateJwtToken(DomainUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var secret = jwtSettings["Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret must be configured.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("userId", user.Id.ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"] ?? "MiniLibrary",
            audience: jwtSettings["Audience"] ?? "MiniLibrary",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(60),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}

/// <summary>
/// Request model for token refresh.
/// </summary>
public record RefreshTokenRequest(string RefreshToken);
