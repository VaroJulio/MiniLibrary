using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Common;

/// <summary>
/// Standard paginated response wrapper for all list API endpoints.
/// Provides consistent structure: data array + pagination metadata.
/// </summary>
/// <typeparam name="T">Type of items in the response.</typeparam>
public sealed class PagedResponse<T>
{
    public List<T> Data { get; init; } = [];
    public PaginationMetadata Pagination { get; init; } = new();

    public PagedResponse() { }

    public PagedResponse(List<T> data, PaginationMetadata pagination)
    {
        Data = data;
        Pagination = pagination;
    }

    /// <summary>
    /// Creates a PagedResponse from a domain PagedResult.
    /// </summary>
    public static PagedResponse<T> FromPagedResult(PagedResult<T> result)
    {
        return new PagedResponse<T>(
            result.Items,
            new PaginationMetadata
            {
                TotalCount = result.TotalCount,
                PageSize = result.PageSize,
                CurrentPage = result.Page,
                TotalPages = result.TotalPages,
                HasNext = result.HasNext,
                HasPrevious = result.HasPrevious
            });
    }

    /// <summary>
    /// Creates a PagedResponse from explicit values.
    /// </summary>
    public static PagedResponse<T> Create(
        List<T> data, int totalCount, int currentPage, int pageSize)
    {
        var totalPages = pageSize > 0 ? (int)Math.Ceiling((double)totalCount / pageSize) : 0;
        return new PagedResponse<T>(
            data,
            new PaginationMetadata
            {
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = currentPage,
                TotalPages = totalPages,
                HasNext = currentPage < totalPages,
                HasPrevious = currentPage > 1
            });
    }
}

/// <summary>
/// Pagination metadata included in all paginated API responses.
/// </summary>
public sealed class PaginationMetadata
{
    public int TotalCount { get; init; }
    public int PageSize { get; init; }
    public int CurrentPage { get; init; }
    public int TotalPages { get; init; }
    public bool HasNext { get; init; }
    public bool HasPrevious { get; init; }
}
