using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Rankings.Queries.GetBookRankings;
using MiniLibrary.Application.Rankings.Queries.GetCategoryRankings;
using MiniLibrary.Application.Rankings.Queries.GetReaderRankings;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Book and reader ranking endpoints (Req 17.1-17.7, 19.5-19.9).
/// Accessible by all authenticated users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class RankingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public RankingsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets book rankings: only books with >= 3 ratings (Req 17.1-17.6).
    /// Supports filters and sorting. Cached for 15 minutes.
    /// </summary>
    /// <param name="category">Filter by category.</param>
    /// <param name="yearFrom">Minimum publication year.</param>
    /// <param name="yearTo">Maximum publication year.</param>
    /// <param name="availableOnly">If true, only show available books.</param>
    /// <param name="sortBy">Sort by: averageRating (default), totalRatings, totalLoans.</param>
    /// <param name="sortDesc">Sort descending (default: true).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Book rankings returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("books")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookRankings(
        [FromQuery] string? category = null,
        [FromQuery] int? yearFrom = null,
        [FromQuery] int? yearTo = null,
        [FromQuery] bool? availableOnly = null,
        [FromQuery] string sortBy = "averageRating",
        [FromQuery] bool sortDesc = true,
        CancellationToken ct = default)
    {
        var query = new GetBookRankingsQuery
        {
            Category = category,
            YearFrom = yearFrom,
            YearTo = yearTo,
            AvailableOnly = availableOnly,
            SortBy = sortBy,
            SortDescending = sortDesc
        };

        var results = await _mediator.Send(query, ct);
        return Ok(new { data = results });
    }

    /// <summary>
    /// Gets reader rankings by return count in period (Req 19.5-19.9).
    /// Includes the requesting member's own position. Cached for 1 hour.
    /// </summary>
    /// <param name="period">Period: 30d, 90d, 12m, all (default: all).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Reader rankings returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("readers")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetReaderRankings(
        [FromQuery] string period = "all",
        CancellationToken ct = default)
    {
        var query = new GetReaderRankingsQuery
        {
            Period = period,
            RequestingUserId = _currentUserService.UserId
        };

        var result = await _mediator.Send(query, ct);
        return Ok(new
        {
            data = result.Rankings,
            myPosition = result.MyPosition
        });
    }

    /// <summary>
    /// Gets category rankings with best-rated book per category (Req 17.5).
    /// Cached for 15 minutes.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Category rankings returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("categories")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetCategoryRankings(CancellationToken ct = default)
    {
        var results = await _mediator.Send(new GetCategoryRankingsQuery(), ct);
        return Ok(new { data = results });
    }
}
