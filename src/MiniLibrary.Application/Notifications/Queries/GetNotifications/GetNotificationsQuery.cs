using MediatR;
using MiniLibrary.Application.Notifications.DTOs;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Notifications.Queries.GetNotifications;

/// <summary>
/// Query to retrieve the member's notifications (max 50, ordered by date desc).
/// </summary>
public sealed record GetNotificationsQuery : IRequest<PagedResult<NotificationResponse>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
