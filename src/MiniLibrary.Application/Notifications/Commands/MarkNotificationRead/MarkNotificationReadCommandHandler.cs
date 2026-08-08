using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Notifications.Commands.MarkNotificationRead;

/// <summary>
/// Handles MarkNotificationReadCommand: marks notification as read if owned by user.
/// </summary>
public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand, Unit>
{
    private readonly INotificationRepository _notificationRepository;

    public MarkNotificationReadCommandHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<Unit> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var notification = await _notificationRepository.GetByIdAsync(request.NotificationId, cancellationToken);
        if (notification is null || notification.UserId != request.UserId)
            throw new NotFoundException("Notification", request.NotificationId);

        notification.MarkAsRead();
        await _notificationRepository.UpdateAsync(notification, cancellationToken);

        return Unit.Value;
    }
}
