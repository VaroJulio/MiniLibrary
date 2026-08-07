namespace MiniLibrary.Application.Notifications.DTOs;

/// <summary>
/// DTO representing a notification in API responses.
/// </summary>
public sealed record NotificationResponse(
    Guid Id,
    string Title,
    string Message,
    string Type,
    bool IsRead,
    DateTime CreatedAt);
