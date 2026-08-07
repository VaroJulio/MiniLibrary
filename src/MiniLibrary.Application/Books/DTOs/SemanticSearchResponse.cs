namespace MiniLibrary.Application.Books.DTOs;

/// <summary>
/// DTO representing the full semantic search response including fallback indicator.
/// </summary>
public sealed record SemanticSearchResponse(
    List<SemanticSearchResultItem> Results,
    bool UsedFallback);

/// <summary>
/// A single book result from semantic search, including its relevance score.
/// </summary>
public sealed record SemanticSearchResultItem(
    Guid Id,
    string Title,
    string Author,
    string Isbn,
    int PublishedYear,
    string Description,
    string Category,
    string Status,
    decimal AverageRating,
    int TotalRatings,
    float RelevanceScore);
