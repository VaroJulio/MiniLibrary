using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Wishlist.Commands.AddToWishlist;
using MiniLibrary.Application.Wishlist.Commands.RemoveFromWishlist;
using MiniLibrary.Application.Wishlist.DTOs;
using MiniLibrary.Application.Wishlist.Queries.GetWishlist;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Wishlist management endpoints (Req 18.1-18.8). Members can manage their own wishlists.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class WishlistController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public WishlistController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the current user's wishlist, paginated (Req 18.2).
    /// Includes book status and date added.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Wishlist returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetWishlist(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Unauthorized();

        var query = new GetWishlistQuery { UserId = userId.Value, Page = page, PageSize = pageSize };
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
    /// Adds a book to the wishlist (Req 18.1). Max 20 entries, duplicates rejected with 409.
    /// </summary>
    /// <param name="request">Book to add.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Book added to wishlist.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Book not found.</response>
    /// <response code="409">Duplicate or wishlist full.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AddToWishlist(
        [FromBody] AddToWishlistRequest request,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Unauthorized();

        var command = new AddToWishlistCommand(request.BookId, userId.Value);
        await _mediator.Send(command, ct);

        return StatusCode(StatusCodes.Status201Created);
    }

    /// <summary>
    /// Removes a book from the wishlist (Req 18.6).
    /// </summary>
    /// <param name="bookId">Book identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Book removed from wishlist.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Entry not found in wishlist.</response>
    [HttpDelete("{bookId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveFromWishlist(Guid bookId, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Unauthorized();

        var command = new RemoveFromWishlistCommand(bookId, userId.Value);
        await _mediator.Send(command, ct);

        return NoContent();
    }
}
