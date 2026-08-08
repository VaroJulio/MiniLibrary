using MediatR;

namespace MiniLibrary.Application.Notifications.Commands.MarkNotificationRead;

/// <summary>
/// Command to mark a notification as read.
/// </summary>
public sealed record MarkNotificationReadCommand(Guid NotificationId, Guid UserId) : IRequest<Unit>;
