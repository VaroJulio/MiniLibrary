using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Gamification.DTOs;
using MiniLibrary.Application.Interfaces;

namespace MiniLibrary.Application.Gamification.Queries.GetLeaderboard;

/// <summary>
/// Handles GetLeaderboardQuery: top 10 members by badge count, cached 1 hour.
/// Uses IGamificationService for DB aggregation.
/// </summary>
public sealed class GetLeaderboardQueryHandler
    : IRequestHandler<GetLeaderboardQuery, List<LeaderboardEntry>>
{
    private readonly IGamificationService _gamificationService;
    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const string CacheKey = "gamification:leaderboard";

    public GetLeaderboardQueryHandler(
        IGamificationService gamificationService,
        ICacheService cacheService)
    {
        _gamificationService = gamificationService;
        _cacheService = cacheService;
    }

    public async Task<List<LeaderboardEntry>> Handle(
        GetLeaderboardQuery request,
        CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<List<LeaderboardEntry>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var results = await _gamificationService.GetLeaderboardAsync(cancellationToken);
        await _cacheService.SetAsync(CacheKey, results, CacheDuration, cancellationToken);
        return results;
    }
}
