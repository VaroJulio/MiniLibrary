using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Gamification.Queries.GetLeaderboard;
using MiniLibrary.Application.Gamification.Queries.GetUserBadges;
using MiniLibrary.Application.Interfaces;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Gamification endpoints (Req 20.3, 20.5, 20.7): badges, progress, and leaderboard.
/// Accessible by all authenticated users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class GamificationController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public GamificationController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the current user's earned badges and progress toward pending badges (Req 20.3).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Badges and progress returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("badges")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyBadges(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Unauthorized();

        var query = new GetUserBadgesQuery(userId.Value, IncludeProgress: true);
        var result = await _mediator.Send(query, ct);

        return Ok(new { data = result });
    }

    /// <summary>
    /// Gets another member's public badges (Req 20.5).
    /// </summary>
    /// <param name="userId">User identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Public badges returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("badges/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUserBadges(Guid userId, CancellationToken ct = default)
    {
        var query = new GetUserBadgesQuery(userId, IncludeProgress: false);
        var result = await _mediator.Send(query, ct);

        return Ok(new { data = result.EarnedBadges });
    }

    /// <summary>
    /// Gets the gamification leaderboard: top 10 members by badge count (Req 20.7).
    /// Cached for 1 hour.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Leaderboard returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet("leaderboard")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetLeaderboard(CancellationToken ct = default)
    {
        var results = await _mediator.Send(new GetLeaderboardQuery(), ct);
        return Ok(new { data = results });
    }
}
