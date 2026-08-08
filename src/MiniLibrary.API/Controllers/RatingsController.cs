using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Ratings.Commands.CreateOrUpdateRating;
using MiniLibrary.Application.Ratings.Commands.DeleteRating;
using MiniLibrary.Application.Ratings.Commands.VoteReviewUseful;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Application.Ratings.Queries.CanRateBook;
using MiniLibrary.Application.Ratings.Queries.GetBookRatings;
using MiniLibrary.Application.Ratings.Queries.GetMyRatings;
using MiniLibrary.Application.Ratings.Queries.GetRecentRatings;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Book ratings and reviews endpoints (Req 16.1-16.8, 20.6, 20.9).
/// Read access for all authenticated users; write access for Members (own ratings only).
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class RatingsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public RatingsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets ratings for a specific book, paginated (Req 16.5).
    /// </summary>
    [HttpGet("books/{bookId:guid}/ratings")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetBookRatings(
        Guid bookId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetBookRatingsQuery { BookId = bookId, Page = page, PageSize = pageSize };
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
    /// Checks if the current user can rate a specific book (has an unrated completed loan).
    /// </summary>
    [HttpGet("books/{bookId:guid}/can-rate")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CanRateBook(Guid bookId, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Unauthorized();

        var query = new CanRateBookQuery { BookId = bookId, UserId = userId.Value };
        var result = await _mediator.Send(query, ct);

        return Ok(new { canRate = result.CanRate, loanId = result.LoanId });
    }

    /// <summary>
    /// Gets the current user's ratings across all books, paginated.
    /// </summary>
    [HttpGet("ratings/my")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyRatings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Unauthorized();

        var query = new GetMyRatingsQuery { UserId = userId.Value, Page = page, PageSize = pageSize };
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
    /// Gets recent community ratings across all books, paginated.
    /// </summary>
    [HttpGet("ratings/recent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetRecentRatings(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetRecentRatingsQuery { Page = page, PageSize = pageSize };
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
    /// Creates or updates a rating for a book (Req 16.1-16.4).
    /// Member must have completed a loan for the book.
    /// Score 1-5, review text max 1000 chars.
    /// </summary>
    [HttpPost("books/{bookId:guid}/ratings")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateOrUpdateRating(
        Guid bookId,
        [FromBody] CreateOrUpdateRatingRequest request,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Unauthorized();

        var command = new CreateOrUpdateRatingCommand
        {
            BookId = bookId,
            UserId = userId.Value,
            Score = request.Score,
            ReviewText = request.ReviewText
        };

        var result = await _mediator.Send(command, ct);
        return Ok(new { data = result });
    }

    /// <summary>
    /// Deletes the current user's rating for a book (Req 16.7).
    /// </summary>
    [HttpDelete("books/{bookId:guid}/ratings")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRating(Guid bookId, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Unauthorized();

        var command = new DeleteRatingCommand(bookId, userId.Value);
        await _mediator.Send(command, ct);

        return NoContent();
    }

    /// <summary>
    /// Votes a review as useful (Req 16.6, 20.9).
    /// One vote per member per review. Cannot vote on own reviews.
    /// </summary>
    [HttpPost("ratings/{ratingId:guid}/useful")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> VoteUseful(Guid ratingId, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
            return Unauthorized();

        var command = new VoteReviewUsefulCommand(ratingId, userId.Value);
        await _mediator.Send(command, ct);

        return NoContent();
    }
}
