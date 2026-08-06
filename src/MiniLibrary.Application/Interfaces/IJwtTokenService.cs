using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Contract for JWT and refresh token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for the given user.
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a refresh token string.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a refresh token and returns the associated user ID if valid.
    /// Returns null if the token is invalid or expired.
    /// </summary>
    Guid? ValidateRefreshToken(string refreshToken);

    /// <summary>
    /// Stores a refresh token for the given user, replacing any existing token.
    /// </summary>
    void StoreRefreshToken(Guid userId, string refreshToken);

    /// <summary>
    /// Revokes the refresh token for the given user.
    /// </summary>
    void RevokeRefreshToken(Guid userId);
}
