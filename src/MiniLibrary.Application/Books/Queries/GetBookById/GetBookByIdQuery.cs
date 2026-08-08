using MediatR;
using MiniLibrary.Application.Books.DTOs;

namespace MiniLibrary.Application.Books.Queries.GetBookById;

/// <summary>
/// Query to retrieve a single book by its unique identifier.
/// </summary>
public record GetBookByIdQuery(Guid Id) : IRequest<BookResponse>;
