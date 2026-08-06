using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Gamification endpoints: badges, achievements, and leaderboard.
/// Accessible by all authenticated users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class GamificationController : ControllerBase
{
    /// <summary>
    /// Gets the current user's badges and progress toward pending badges.
    /// </summary>
    [HttpGet("badges")]
    public IActionResult GetMyBadges()
    {
        // Implementation in Task 16.2
        return Ok();
    }

    /// <summary>
    /// Gets a user's public badges.
    /// </summary>
    /// <param name="userId">User identifier</param>
    [HttpGet("badges/{userId:guid}")]
    public IActionResult GetUserBadges(Guid userId)
    {
        // Implementation in Task 16.2
        return Ok();
    }

    /// <summary>
    /// Gets the gamification leaderboard (top 10 by badge count).
    /// </summary>
    [HttpGet("leaderboard")]
    public IActionResult GetLeaderboard()
    {
        // Implementation in Task 16.2
        return Ok();
    }
}
