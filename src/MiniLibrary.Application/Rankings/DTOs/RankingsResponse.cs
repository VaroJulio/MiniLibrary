namespace MiniLibrary.Application.Rankings.DTOs;

/// <summary>
/// A single book in the rankings list.
/// </summary>
public sealed record BookRankingItem(
    int Position,
    Guid BookId,
    string Title,
    string Author,
    string Category,
    decimal AverageRating,
    int TotalRatings,
    int TotalLoans,
    string Status);

/// <summary>
/// A single reader in the rankings list.
/// </summary>
public sealed record ReaderRankingItem(
    int Position,
    Guid UserId,
    string Name,
    int BooksReadInPeriod,
    string MostReadCategory,
    decimal AverageRatingGiven);

/// <summary>
/// A category in the rankings list.
/// </summary>
public sealed record CategoryRankingItem(
    string Category,
    decimal AverageRating,
    int TotalBooks,
    string BestBookTitle,
    string BestBookAuthor);
