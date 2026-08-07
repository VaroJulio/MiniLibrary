using MediatR;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Rankings.Queries.GetReaderRankings;

/// <summary>
/// Handles GetReaderRankingsQuery with 1-hour caching.
/// Includes the requesting user's position in the ranking.
/// </summary>
public sealed class GetReaderRankingsQueryHandler
    : IRequestHandler<GetReaderRankingsQuery, ReaderRankingsResponse>
{
    private readonly IRankingsService _rankingsService;
    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);

    public GetReaderRankingsQueryHandler(
        IRankingsService rankingsService,
        ICacheService cacheService)
    {
        _rankingsService = rankingsService;
        _cacheService = cacheService;
    }

    public async Task<ReaderRankingsResponse> Handle(
        GetReaderRankingsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"rankings:readers:{request.Period}";

        var cached = await _cacheService.GetAsync<List<ReaderRankingItem>>(cacheKey, cancellationToken);
        List<ReaderRankingItem> rankings;

        if (cached is not null)
        {
            rankings = cached;
        }
        else
        {
            rankings = await _rankingsService.GetReaderRankingsAsync(request.Period, cancellationToken);
            await _cacheService.SetAsync(cacheKey, rankings, CacheDuration, cancellationToken);
        }

        // Find requesting user's position
        int? myPosition = null;
        if (request.RequestingUserId.HasValue)
        {
            var myEntry = rankings.FirstOrDefault(r => r.UserId == request.RequestingUserId.Value);
            myPosition = myEntry?.Position;
        }

        return new ReaderRankingsResponse(rankings, myPosition);
    }
}
