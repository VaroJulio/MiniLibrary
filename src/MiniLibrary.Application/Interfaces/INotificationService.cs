using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for delivering notifications to users (in-app and email).
/// </summary>
public interface INotificationService
{
    /// <summary>Creates an in-app notification for the specified user.</summary>
    Task SendInAppAsync(Guid userId, string title, string message, NotificationType type, CancellationToken ct);

    /// <summary>Sends an HTML email notification to the specified address.</summary>
    Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken ct);
}
