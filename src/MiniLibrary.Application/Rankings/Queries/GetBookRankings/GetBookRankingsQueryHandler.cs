using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Rankings.Queries.GetBookRankings;

/// <summary>
/// Handles GetBookRankingsQuery with 15-minute caching.
/// Delegates aggregation to IRankingsService.
/// </summary>
public sealed class GetBookRankingsQueryHandler
    : IRequestHandler<GetBookRankingsQuery, List<BookRankingItem>>
{
    private readonly IRankingsService _rankingsService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetBookRankingsQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public GetBookRankingsQueryHandler(
        IRankingsService rankingsService,
        ICacheService cacheService,
        ILogger<GetBookRankingsQueryHandler> logger)
    {
        _rankingsService = rankingsService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<BookRankingItem>> Handle(
        GetBookRankingsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"rankings:books:{request.Category}:{request.YearFrom}:{request.YearTo}:{request.AvailableOnly}:{request.SortBy}:{request.SortDescending}";

        var cached = await _cacheService.GetAsync<List<BookRankingItem>>(cacheKey, cancellationToken);
        if (cached is not null)
            return cached;

        var results = await _rankingsService.GetBookRankingsAsync(
            request.Category,
            request.YearFrom,
            request.YearTo,
            request.AvailableOnly,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        await _cacheService.SetAsync(cacheKey, results, CacheDuration, cancellationToken);
        return results;
    }
}
