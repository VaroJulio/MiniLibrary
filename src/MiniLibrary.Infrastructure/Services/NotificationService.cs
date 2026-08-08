using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Configuration;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// Implementation of <see cref="INotificationService"/>.
/// Creates in-app notifications and delivers email via SMTP (Gmail).
/// </summary>
public sealed class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly EmailOptions _emailOptions;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IOptions<EmailOptions> emailOptions,
        ILogger<NotificationService> logger)
    {
        _notificationRepository = notificationRepository;
        _emailOptions = emailOptions.Value;
        _logger = logger;
    }

    public async Task SendInAppAsync(
        Guid userId, string title, string message, NotificationType type, CancellationToken ct)
    {
        var notification = Notification.Create(userId, title, message, type);
        await _notificationRepository.AddAsync(notification, ct);
    }

    public async Task SendEmailAsync(string email, string subject, string htmlBody, CancellationToken ct)
    {
        if (!_emailOptions.IsConfigured)
        {
            _logger.LogWarning(
                "Email settings not configured. Skipping email to {Email}: {Subject}",
                email, subject);
            return;
        }

        try
        {
            using var smtpClient = new SmtpClient(_emailOptions.SmtpHost, _emailOptions.SmtpPort)
            {
                Credentials = new NetworkCredential(_emailOptions.SenderEmail, _emailOptions.AppPassword),
                EnableSsl = _emailOptions.UseTls,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 15_000
            };

            using var message = new MailMessage
            {
                From = new MailAddress(_emailOptions.SenderEmail, _emailOptions.SenderName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(email));

            await smtpClient.SendMailAsync(message, ct);

            _logger.LogInformation("Email sent successfully to {Email}: {Subject}", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", email, subject);
            // Don't throw — email delivery failure should not break the main flow.
        }
    }
}
