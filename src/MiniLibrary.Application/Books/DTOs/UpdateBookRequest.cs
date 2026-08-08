namespace MiniLibrary.Application.Books.DTOs;

/// <summary>
/// Request body for updating an existing book in the catalog.
/// </summary>
public record UpdateBookRequest(
    string Title,
    string Author,
    string Isbn,
    int PublishedYear,
    string Description,
    string Category);
