using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Domain.Common;

/// <summary>
/// Criteria used to search and filter the book catalog.
/// </summary>
public sealed record SearchCriteria
{
    /// <summary>Free-text search term matched against title, author, ISBN, and category.</summary>
    public string? Query { get; init; }

    /// <summary>Filter by exact category name.</summary>
    public string? Category { get; init; }

    /// <summary>Filter by book availability status.</summary>
    public BookStatus? Status { get; init; }

    /// <summary>Minimum publication year (inclusive).</summary>
    public int? MinYear { get; init; }

    /// <summary>Maximum publication year (inclusive).</summary>
    public int? MaxYear { get; init; }

    /// <summary>1-based page number (default 1).</summary>
    public int Page { get; init; } = 1;

    /// <summary>Page size capped at 100 (default 20).</summary>
    public int PageSize { get; init; } = 20;

    /// <summary>Field to sort by (e.g., "title", "author", "publishedYear").</summary>
    public string? SortBy { get; init; }

    /// <summary>When true, sort in descending order.</summary>
    public bool SortDescending { get; init; }
}
