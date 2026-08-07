using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="INotificationService"/>.
/// Creates in-app notifications and logs email delivery (email integration placeholder).
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _logger = logger;
    }

    public async Task SendInAppAsync(
        Guid userId, string title, string message, NotificationType type, CancellationToken ct)
    {
        var notification = Notification.Create(userId, title, message, type);
        await _notificationRepository.AddAsync(notification, ct);
    }

    public Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken ct)
    {
        // Placeholder: log email delivery. In production, integrate with SendGrid/SES/SMTP.
        _logger.LogInformation("Email notification to {Email}: {Subject}", email, subject);
        return Task.CompletedTask;
    }
}
