using MediatR;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Application.Books.Queries.SearchBooks;

/// <summary>
/// Query to search books with text search, optional filters, and pagination.
/// Supports search across title, author, ISBN, and category.
/// </summary>
public record SearchBooksQuery : IRequest<PagedResult<BookResponse>>
{
    /// <summary>Free-text search term matched against title, author, ISBN, and category.</summary>
    public string? SearchTerm { get; init; }

    /// <summary>Filter by exact category name.</summary>
    public string? Category { get; init; }

    /// <summary>Filter by book availability status.</summary>
    public BookStatus? Status { get; init; }

    /// <summary>Minimum publication year (inclusive).</summary>
    public int? YearFrom { get; init; }

    /// <summary>Maximum publication year (inclusive).</summary>
    public int? YearTo { get; init; }

    /// <summary>Field to sort by (e.g., "title", "author", "publishedYear").</summary>
    public string? SortBy { get; init; }

    /// <summary>When true, sort in descending order.</summary>
    public bool SortDescending { get; init; }

    /// <summary>1-based page number (default 1).</summary>
    public int Page { get; init; } = PaginationParams.DefaultPage;

    /// <summary>Page size (default 20, max 100).</summary>
    public int PageSize { get; init; } = PaginationParams.DefaultPageSize;
}
