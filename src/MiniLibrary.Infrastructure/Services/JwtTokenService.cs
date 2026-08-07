using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// JWT access token and refresh token generation/validation service.
/// </summary>
public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _configuration;
    private readonly ConcurrentDictionary<string, RefreshTokenEntry> _refreshTokens = new();

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <inheritdoc />
    public string GenerateAccessToken(User user)
    {
        var secret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer = _configuration["Jwt:Issuer"] ?? "MiniLibrary";
        var audience = _configuration["Jwt:Audience"] ?? "MiniLibrary";
        var expirationMinutes = int.TryParse(_configuration["Jwt:ExpirationMinutes"], out var exp) ? exp : 60;

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("userId", user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("role", user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <inheritdoc />
    public string GenerateRefreshToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    /// <inheritdoc />
    public Guid? ValidateRefreshToken(string refreshToken)
    {
        if (!_refreshTokens.TryGetValue(refreshToken, out var entry))
            return null;

        if (entry.ExpiresAt < DateTime.UtcNow)
        {
            _refreshTokens.TryRemove(refreshToken, out _);
            return null;
        }

        return entry.UserId;
    }

    /// <inheritdoc />
    public void StoreRefreshToken(Guid userId, string refreshToken)
    {
        var refreshExpirationDays = int.TryParse(_configuration["Jwt:RefreshExpirationDays"], out var days) ? days : 7;

        // Remove any existing refresh tokens for this user
        var existingTokens = _refreshTokens
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var existingToken in existingTokens)
        {
            _refreshTokens.TryRemove(existingToken, out _);
        }

        var entry = new RefreshTokenEntry
        {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshExpirationDays)
        };

        _refreshTokens[refreshToken] = entry;
    }

    /// <inheritdoc />
    public void RevokeRefreshToken(Guid userId)
    {
        var tokensToRemove = _refreshTokens
            .Where(kvp => kvp.Value.UserId == userId)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var token in tokensToRemove)
        {
            _refreshTokens.TryRemove(token, out _);
        }
    }

    private sealed class RefreshTokenEntry
    {
        public Guid UserId { get; init; }
        public DateTime ExpiresAt { get; init; }
    }
}
