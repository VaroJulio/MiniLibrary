using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for Notification entities.
/// </summary>
public interface INotificationRepository
{
    Task<PagedResult<Notification>> GetUserNotificationsAsync(Guid userId, PaginationParams paging, CancellationToken ct);
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Notification notification, CancellationToken ct);
    Task UpdateAsync(Notification notification, CancellationToken ct);
}
