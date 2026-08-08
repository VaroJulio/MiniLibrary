using MediatR;

namespace MiniLibrary.Application.Books.Commands.DeleteBook;

/// <summary>
/// Command to soft-delete a book from the library catalog.
/// </summary>
public record DeleteBookCommand(Guid Id) : IRequest;
