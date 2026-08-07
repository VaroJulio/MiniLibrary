using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for computing ranking data from the database.
/// </summary>
public interface IRankingsService
{
    /// <summary>
    /// Gets book rankings: only books with >= 3 ratings, with optional filters and sorting.
    /// </summary>
    Task<List<BookRankingItem>> GetBookRankingsAsync(
        string? category,
        int? yearFrom,
        int? yearTo,
        bool? availableOnly,
        string sortBy,
        bool sortDescending,
        CancellationToken ct);

    /// <summary>
    /// Gets reader rankings by return count in the specified period.
    /// </summary>
    Task<List<ReaderRankingItem>> GetReaderRankingsAsync(
        string period,
        CancellationToken ct);

    /// <summary>
    /// Gets category rankings with best-rated book per category.
    /// </summary>
    Task<List<CategoryRankingItem>> GetCategoryRankingsAsync(CancellationToken ct);
}
