using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Book ratings and reviews endpoints.
/// Read access for all authenticated users; write access for Members (own ratings only).
/// </summary>
[ApiController]
[Route("api")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class RatingsController : ControllerBase
{
    /// <summary>
    /// Gets ratings for a specific book (paginated).
    /// </summary>
    /// <param name="bookId">Book identifier</param>
    [HttpGet("books/{bookId:guid}/ratings")]
    public IActionResult GetBookRatings(Guid bookId)
    {
        // Implementation in Task 13.2
        return Ok();
    }

    /// <summary>
    /// Creates or updates a rating for a book. Requires Member role (must have read the book).
    /// </summary>
    /// <param name="bookId">Book identifier</param>
    [HttpPost("books/{bookId:guid}/ratings")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    public IActionResult CreateOrUpdateRating(Guid bookId)
    {
        // Implementation in Task 13.2
        return Ok();
    }

    /// <summary>
    /// Deletes the current user's rating for a book.
    /// </summary>
    /// <param name="bookId">Book identifier</param>
    [HttpDelete("books/{bookId:guid}/ratings")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    public IActionResult DeleteRating(Guid bookId)
    {
        // Implementation in Task 13.2
        return NoContent();
    }

    /// <summary>
    /// Votes a review as useful. Members only; cannot vote on own reviews.
    /// </summary>
    /// <param name="ratingId">Rating identifier</param>
    [HttpPost("ratings/{ratingId:guid}/useful")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    public IActionResult VoteUseful(Guid ratingId)
    {
        // Implementation in Task 13.2
        return Ok();
    }
}
