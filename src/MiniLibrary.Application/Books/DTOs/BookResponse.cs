namespace MiniLibrary.Application.Books.DTOs;

/// <summary>
/// DTO representing a book in API responses.
/// </summary>
public record BookResponse(
    Guid Id,
    string Title,
    string Author,
    string Isbn,
    int PublishedYear,
    string Description,
    string Category,
    string Status,
    decimal AverageRating,
    int TotalRatings);
