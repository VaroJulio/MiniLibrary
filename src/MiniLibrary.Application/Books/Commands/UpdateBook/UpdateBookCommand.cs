using MediatR;
using MiniLibrary.Application.Books.DTOs;

namespace MiniLibrary.Application.Books.Commands.UpdateBook;

/// <summary>
/// Command to update an existing book in the library catalog.
/// </summary>
public record UpdateBookCommand(
    Guid Id,
    string Title,
    string Author,
    string Isbn,
    int PublishedYear,
    string Description,
    string Category) : IRequest<BookResponse>;
