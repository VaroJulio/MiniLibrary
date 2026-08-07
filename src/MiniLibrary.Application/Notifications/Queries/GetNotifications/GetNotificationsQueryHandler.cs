using MediatR;
using MiniLibrary.Application.Notifications.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Notifications.Queries.GetNotifications;

/// <summary>
/// Handles GetNotificationsQuery: returns paginated notifications (max 50, desc by date).
/// </summary>
public sealed class GetNotificationsQueryHandler
    : IRequestHandler<GetNotificationsQuery, PagedResult<NotificationResponse>>
{
    private readonly INotificationRepository _notificationRepository;

    public GetNotificationsQueryHandler(INotificationRepository notificationRepository)
    {
        _notificationRepository = notificationRepository;
    }

    public async Task<PagedResult<NotificationResponse>> Handle(
        GetNotificationsQuery request,
        CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, Math.Min(request.PageSize, 50));
        var result = await _notificationRepository.GetUserNotificationsAsync(
            request.UserId, paging, cancellationToken);

        var items = result.Items.Select(n => new NotificationResponse(
            n.Id,
            n.Title,
            n.Message,
            n.Type.ToString(),
            n.IsRead,
            n.CreatedAt)).ToList();

        return new PagedResult<NotificationResponse>(
            items, result.TotalCount, result.Page, result.PageSize);
    }
}
