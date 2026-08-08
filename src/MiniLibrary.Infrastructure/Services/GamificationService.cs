using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Gamification.DTOs;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// EF Core implementation of <see cref="IGamificationService"/>.
/// </summary>
public sealed class GamificationService : IGamificationService
{
    private readonly AppDbContext _context;

    public GamificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<LeaderboardEntry>> GetLeaderboardAsync(CancellationToken ct)
    {
        var leaders = await _context.Badges
            .Include(b => b.User)
            .GroupBy(b => new { b.UserId, b.User.FullName })
            .Select(g => new { g.Key.UserId, Name = g.Key.FullName, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .Take(10)
            .ToListAsync(ct);

        return leaders.Select((l, idx) => new LeaderboardEntry(
            idx + 1, l.UserId, l.Name, l.Count)).ToList();
    }
}
