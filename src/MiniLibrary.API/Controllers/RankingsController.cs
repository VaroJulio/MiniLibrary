using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Book and reader ranking endpoints. Accessible by all authenticated users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class RankingsController : ControllerBase
{
    /// <summary>
    /// Gets the book ranking (filtered, sorted, paginated).
    /// </summary>
    [HttpGet("books")]
    public IActionResult GetBookRankings()
    {
        // Implementation in Task 14.1
        return Ok();
    }

    /// <summary>
    /// Gets the reader ranking by activity.
    /// </summary>
    [HttpGet("readers")]
    public IActionResult GetReaderRankings()
    {
        // Implementation in Task 14.2
        return Ok();
    }

    /// <summary>
    /// Gets category rankings.
    /// </summary>
    [HttpGet("categories")]
    public IActionResult GetCategoryRankings()
    {
        // Implementation in Task 14.2
        return Ok();
    }
}
