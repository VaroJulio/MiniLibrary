using AutoMapper;
using MediatR;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.Queries.SearchBooks;

/// <summary>
/// Handles the SearchBooksQuery by constructing SearchCriteria from query parameters
/// and delegating to IBookRepository.SearchAsync.
/// </summary>
public class SearchBooksQueryHandler : IRequestHandler<SearchBooksQuery, PagedResult<BookResponse>>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public SearchBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<BookResponse>> Handle(SearchBooksQuery request, CancellationToken cancellationToken)
    {
        var criteria = new SearchCriteria
        {
            Query = request.SearchTerm,
            Category = request.Category,
            Status = request.Status,
            MinYear = request.YearFrom,
            MaxYear = request.YearTo,
            SortBy = request.SortBy,
            SortDescending = request.SortDescending,
            Page = request.Page,
            PageSize = request.PageSize
        };

        var pagedBooks = await _bookRepository.SearchAsync(criteria, cancellationToken);

        var bookResponses = _mapper.Map<List<BookResponse>>(pagedBooks.Items);

        return new PagedResult<BookResponse>(
            bookResponses,
            pagedBooks.TotalCount,
            pagedBooks.Page,
            pagedBooks.PageSize);
    }
}
