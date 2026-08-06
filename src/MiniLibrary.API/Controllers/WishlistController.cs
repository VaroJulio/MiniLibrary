using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Wishlist management endpoints. Members can manage their own wishlists.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class WishlistController : ControllerBase
{
    /// <summary>
    /// Gets the current user's wishlist (paginated).
    /// </summary>
    [HttpGet]
    public IActionResult GetWishlist()
    {
        // Implementation in Task 15.1
        return Ok();
    }

    /// <summary>
    /// Adds a book to the current user's wishlist. Max 20 entries.
    /// </summary>
    [HttpPost]
    public IActionResult AddToWishlist()
    {
        // Implementation in Task 15.1
        return StatusCode(201);
    }

    /// <summary>
    /// Removes a book from the current user's wishlist.
    /// </summary>
    /// <param name="bookId">Book identifier</param>
    [HttpDelete("{bookId:guid}")]
    public IActionResult RemoveFromWishlist(Guid bookId)
    {
        // Implementation in Task 15.1
        return NoContent();
    }
}
