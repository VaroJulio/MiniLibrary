using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IBadgeRepository"/>.
/// Badges are ordered by EarnedAt descending.
/// </summary>
public sealed class BadgeRepository : IBadgeRepository
{
    private readonly AppDbContext _context;

    public BadgeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Badge>> GetUserBadgesAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Badges
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.EarnedAt)
            .ToListAsync(ct);
    }

    public async Task<bool> HasBadgeAsync(Guid userId, string badgeType, CancellationToken ct)
    {
        if (!Enum.TryParse<BadgeType>(badgeType, ignoreCase: true, out var parsedType))
            return false;

        return await _context.Badges
            .AnyAsync(b => b.UserId == userId && b.BadgeType == parsedType, ct);
    }

    public async Task AddAsync(Badge badge, CancellationToken ct)
    {
        await _context.Badges.AddAsync(badge, ct);
        await _context.SaveChangesAsync(ct);
    }
}
