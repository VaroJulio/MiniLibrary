using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Provides access to the currently authenticated user's identity information.
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the internal user ID of the authenticated user.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the role of the authenticated user.
    /// </summary>
    UserRole? Role { get; }

    /// <summary>
    /// Gets the email of the authenticated user.
    /// </summary>
    string? Email { get; }

    /// <summary>
    /// Indicates whether the current request is from an authenticated user.
    /// </summary>
    bool IsAuthenticated { get; }
}
