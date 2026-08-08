using AutoMapper;
using MediatR;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.Queries.GetBookById;

/// <summary>
/// Handles retrieval of a book by its ID.
/// Returns the mapped BookResponse or throws NotFoundException.
/// </summary>
public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<BookResponse> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookRepository.GetByIdAsync(request.Id, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException("Book", request.Id);
        }

        return _mapper.Map<BookResponse>(book);
    }
}
