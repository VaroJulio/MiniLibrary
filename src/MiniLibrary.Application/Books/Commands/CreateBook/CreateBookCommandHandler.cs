using AutoMapper;
using MediatR;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.Commands.CreateBook;

/// <summary>
/// Handles creation of a new book in the catalog.
/// Validates ISBN uniqueness and sets initial status to Available.
/// </summary>
public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, BookResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public CreateBookCommandHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookResponse> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        // Check ISBN uniqueness
        var existingBook = await _bookRepository.GetByIsbnAsync(request.Isbn, cancellationToken);
        if (existingBook is not null)
        {
            throw new ConflictException($"A book with ISBN '{request.Isbn}' already exists.");
        }

        // Create the book entity (status defaults to Available via factory method)
        var book = Book.Create(
            title: request.Title,
            author: request.Author,
            isbn: request.Isbn,
            publishedYear: request.PublishedYear,
            description: request.Description,
            category: request.Category);

        await _bookRepository.AddAsync(book, cancellationToken);

        return _mapper.Map<BookResponse>(book);
    }
}
