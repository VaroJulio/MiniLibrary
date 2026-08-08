using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Ratings.Commands.CreateOrUpdateRating;
using MiniLibrary.Application.Ratings.Commands.DeleteRating;
using MiniLibrary.Application.Ratings.Commands.VoteReviewUseful;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Application.Ratings.Queries.GetBookRatings;

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
    /// <param name="bookId">Book identifier.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of ratings with author names.</returns>
    /// <response code="200">Ratings returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
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
    /// Creates or updates a rating for a book (Req 16.1-16.4).
    /// Member must have completed a loan for the book.
    /// Score 1-5, review text max 1000 chars.
    /// </summary>
    /// <param name="bookId">Book identifier.</param>
    /// <param name="request">Rating data (score, reviewText).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Rating created or updated successfully.</response>
    /// <response code="400">Invalid score or review text.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User has not completed a loan for this book.</response>
    /// <response code="404">Book not found.</response>
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
    /// <param name="bookId">Book identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Rating deleted successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Rating not found.</response>
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
    /// <param name="ratingId">Rating identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Vote registered successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">Cannot vote on own review.</response>
    /// <response code="404">Rating not found.</response>
    /// <response code="409">Already voted on this review.</response>
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
