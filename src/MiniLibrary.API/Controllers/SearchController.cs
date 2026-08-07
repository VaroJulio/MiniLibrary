using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Queries.SearchBooks;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Provides text-based book search with filters and pagination.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class SearchController : ControllerBase
{
    private readonly IMediator _mediator;

    public SearchController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Searches books by text query with optional filters and pagination (Req 3.1-3.8).
    /// Text search matches against title, author, ISBN, and category.
    /// </summary>
    /// <param name="q">Free-text search term (1-200 chars). If empty, returns all books paginated.</param>
    /// <param name="category">Filter by exact category name.</param>
    /// <param name="status">Filter by book status (Available, CheckedOut).</param>
    /// <param name="yearFrom">Minimum publication year (inclusive).</param>
    /// <param name="yearTo">Maximum publication year (inclusive).</param>
    /// <param name="sortBy">Sort field: title, author, publishedYear (default: relevance).</param>
    /// <param name="sortDesc">Sort descending when true (default: false).</param>
    /// <param name="page">Page number, 1-based (default: 1).</param>
    /// <param name="pageSize">Items per page, 1-100 (default: 20).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of matching books.</returns>
    /// <response code="200">Search results returned successfully.</response>
    /// <response code="400">Invalid query length or filter values.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("books")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SearchBooks(
        [FromQuery] string? q = null,
        [FromQuery] string? category = null,
        [FromQuery] BookStatus? status = null,
        [FromQuery] int? yearFrom = null,
        [FromQuery] int? yearTo = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDesc = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        // Validate query length (Req 3.1)
        if (q is not null && q.Length > 200)
        {
            return BadRequest(new { error = "Search query must not exceed 200 characters." });
        }

        // Validate pagination (Req 3.7)
        if (page < 1)
        {
            return BadRequest(new { error = "Page must be greater than or equal to 1." });
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest(new { error = "Page size must be between 1 and 100." });
        }

        var query = new SearchBooksQuery
        {
            SearchTerm = q,
            Category = category,
            Status = status,
            YearFrom = yearFrom,
            YearTo = yearTo,
            SortBy = sortBy,
            SortDescending = sortDesc,
            Page = page,
            PageSize = pageSize
        };

        var result = await _mediator.Send(query, ct);

        return Ok(new
        {
            data = result.Items,
            pagination = new
            {
                totalCount = result.TotalCount,
                pageSize = result.PageSize,
                currentPage = result.Page,
                totalPages = result.TotalPages,
                hasNext = result.HasNext,
                hasPrevious = result.HasPrevious
            }
        });
    }

    /// <summary>
    /// Performs semantic search using natural language.
    /// </summary>
    [HttpGet("semantic")]
    public IActionResult SemanticSearch()
    {
        // Implementation in Task 9.2
        return Ok();
    }
}
