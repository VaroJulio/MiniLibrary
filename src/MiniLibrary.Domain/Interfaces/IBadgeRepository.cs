using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for Badge entities (gamification).
/// </summary>
public interface IBadgeRepository
{
    Task<List<Badge>> GetUserBadgesAsync(Guid userId, CancellationToken ct);
    Task<bool> HasBadgeAsync(Guid userId, string badgeType, CancellationToken ct);
    Task AddAsync(Badge badge, CancellationToken ct);
}
