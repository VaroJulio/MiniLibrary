namespace MiniLibrary.Application.Books.DTOs;

/// <summary>
/// Request body for creating a new book in the catalog.
/// </summary>
public record CreateBookRequest(
    string Title,
    string Author,
    string Isbn,
    int PublishedYear,
    string Description,
    string Category);
