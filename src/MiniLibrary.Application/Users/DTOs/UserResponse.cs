namespace MiniLibrary.Application.Users.DTOs;

/// <summary>
/// DTO representing a user in API responses.
/// </summary>
public sealed record UserResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    DateTime CreatedAt);

/// <summary>
/// DTO for the current user's profile with additional preference details.
/// </summary>
public sealed record UserProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    string Role,
    bool EmailAlertsExpiration,
    bool EmailAlertsAvailability,
    DateTime CreatedAt);
