using AutoMapper;
using MediatR;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.Commands.UpdateBook;

/// <summary>
/// Handles updating an existing book in the catalog.
/// Validates book existence and ISBN uniqueness if changed.
/// </summary>
public class UpdateBookCommandHandler : IRequestHandler<UpdateBookCommand, BookResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public UpdateBookCommandHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookResponse> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        // Get the book by Id
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException("Book", request.Id);
        }

        // Check ISBN uniqueness if changed
        if (!string.Equals(book.ISBN, request.Isbn, StringComparison.OrdinalIgnoreCase))
        {
            var existingBook = await _bookRepository.GetByIsbnAsync(request.Isbn, cancellationToken);
            if (existingBook is not null)
            {
                throw new ConflictException($"A book with ISBN '{request.Isbn}' already exists.");
            }
        }

        // Update book fields
        book.Update(
            title: request.Title,
            author: request.Author,
            isbn: request.Isbn,
            publishedYear: request.PublishedYear,
            description: request.Description,
            category: request.Category);

        await _bookRepository.UpdateAsync(book, cancellationToken);

        return _mapper.Map<BookResponse>(book);
    }
}
