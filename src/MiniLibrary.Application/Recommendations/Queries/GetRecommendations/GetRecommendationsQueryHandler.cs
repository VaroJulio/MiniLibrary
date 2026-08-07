using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Recommendations.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Recommendations.Queries.GetRecommendations;

/// <summary>
/// Handles GetRecommendationsQuery by:
/// 1. Checking cached recommendations (1 hour TTL per member)
/// 2. Loading user's loan history and available catalog
/// 3. Excluding books already read or currently on loan
/// 4. Calling IRecommendationService for AI-powered suggestions
/// 5. Caching the result
/// </summary>
public sealed class GetRecommendationsQueryHandler
    : IRequestHandler<GetRecommendationsQuery, List<RecommendationResponse>>
{
    private readonly IRecommendationService _recommendationService;
    private readonly ILoanRepository _loanRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetRecommendationsQueryHandler> _logger;

    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(1);
    private const string CacheKeyPrefix = "recommendations:";

    public GetRecommendationsQueryHandler(
        IRecommendationService recommendationService,
        ILoanRepository loanRepository,
        IBookRepository bookRepository,
        ICacheService cacheService,
        ILogger<GetRecommendationsQueryHandler> logger)
    {
        _recommendationService = recommendationService;
        _loanRepository = loanRepository;
        _bookRepository = bookRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<List<RecommendationResponse>> Handle(
        GetRecommendationsQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"{CacheKeyPrefix}{request.UserId}";

        // 1. Check cache
        var cached = await _cacheService.GetAsync<List<RecommendationResponse>>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Returning cached recommendations for user {UserId}.", request.UserId);
            return cached;
        }

        // 2. Load user's full loan history (up to 200 for recommendation context)
        var historyResult = await _loanRepository.GetUserHistoryAsync(
            request.UserId,
            new PaginationParams(1, 200),
            cancellationToken);
        var history = historyResult.Items;

        // 3. Determine which books to exclude (already read or currently on loan)
        var excludedBookIds = history
            .Select(l => l.BookId)
            .ToHashSet();

        // 4. Load available catalog (excluding already-read books)
        var allBooksResult = await _bookRepository.SearchAsync(
            new SearchCriteria { Page = 1, PageSize = 100, Status = BookStatus.Available },
            cancellationToken);

        var availableCatalog = allBooksResult.Items
            .Where(b => !excludedBookIds.Contains(b.Id))
            .ToList();

        // 5. Call recommendation service
        var recommendations = await _recommendationService.GetRecommendationsAsync(
            request.UserId,
            history,
            availableCatalog,
            cancellationToken);

        // 6. Filter out any recommendations that the user has already read (double safety)
        var filteredRecommendations = recommendations
            .Where(r => !excludedBookIds.Contains(r.BookId))
            .Select(r => new RecommendationResponse(
                r.BookId,
                r.Title,
                r.Author,
                r.Category,
                r.Justification))
            .ToList();

        // 7. Cache the result
        await _cacheService.SetAsync(cacheKey, filteredRecommendations, CacheDuration, cancellationToken);

        _logger.LogInformation("Generated {Count} recommendations for user {UserId}.", filteredRecommendations.Count, request.UserId);
        return filteredRecommendations;
    }

    /// <summary>
    /// Returns the cache key for a given user. Used by loan handlers to invalidate cache.
    /// </summary>
    public static string GetCacheKey(Guid userId) => $"{CacheKeyPrefix}{userId}";
}
