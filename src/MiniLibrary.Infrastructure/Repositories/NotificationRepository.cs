using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="INotificationRepository"/>.
/// Notifications are ordered by CreatedAt descending, with a max page size of 50.
/// </summary>
public sealed class NotificationRepository : INotificationRepository
{
    private const int MaxPageSize = 50;

    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Notification>> GetUserNotificationsAsync(Guid userId, PaginationParams paging, CancellationToken ct)
    {
        var baseQuery = _context.Notifications.AsNoTracking()
            .Where(n => n.UserId == userId);

        var totalCount = await baseQuery.CountAsync(ct);

        var pageSize = Math.Min(paging.PageSize, MaxPageSize);

        var items = await baseQuery
            .OrderByDescending(n => n.CreatedAt)
            .Skip((paging.Page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Notification>(items, totalCount, paging.Page, pageSize);
    }

    public async Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id, ct);
    }

    public async Task AddAsync(Notification notification, CancellationToken ct)
    {
        await _context.Notifications.AddAsync(notification, ct);
    }

    public async Task UpdateAsync(Notification notification, CancellationToken ct)
    {
        _context.Notifications.Update(notification);
        await Task.CompletedTask;
    }
}
