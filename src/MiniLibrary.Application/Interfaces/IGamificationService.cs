using MiniLibrary.Application.Gamification.DTOs;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for gamification leaderboard aggregation.
/// </summary>
public interface IGamificationService
{
    /// <summary>
    /// Gets top 10 members by badge count.
    /// </summary>
    Task<List<LeaderboardEntry>> GetLeaderboardAsync(CancellationToken ct);
}
