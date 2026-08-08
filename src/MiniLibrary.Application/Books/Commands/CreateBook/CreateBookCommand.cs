using MediatR;
using MiniLibrary.Application.Books.DTOs;

namespace MiniLibrary.Application.Books.Commands.CreateBook;

/// <summary>
/// Command to create a new book in the library catalog.
/// </summary>
public record CreateBookCommand(
    string Title,
    string Author,
    string Isbn,
    int PublishedYear,
    string Description,
    string Category) : IRequest<BookResponse>;
