using MediatR;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Rankings.Queries.GetCategoryRankings;

/// <summary>
/// Handles GetCategoryRankingsQuery with 15-minute caching.
/// </summary>
public sealed class GetCategoryRankingsQueryHandler
    : IRequestHandler<GetCategoryRankingsQuery, List<CategoryRankingItem>>
{
    private readonly IRankingsService _rankingsService;
    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);
    private const string CacheKey = "rankings:categories";

    public GetCategoryRankingsQueryHandler(
        IRankingsService rankingsService,
        ICacheService cacheService)
    {
        _rankingsService = rankingsService;
        _cacheService = cacheService;
    }

    public async Task<List<CategoryRankingItem>> Handle(
        GetCategoryRankingsQuery request,
        CancellationToken cancellationToken)
    {
        var cached = await _cacheService.GetAsync<List<CategoryRankingItem>>(CacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var results = await _rankingsService.GetCategoryRankingsAsync(cancellationToken);
        await _cacheService.SetAsync(CacheKey, results, CacheDuration, cancellationToken);
        return results;
    }
}
