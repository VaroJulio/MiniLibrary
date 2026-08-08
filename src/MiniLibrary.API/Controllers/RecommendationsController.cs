using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Recommendations.Queries.GetRecommendations;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// AI-powered book recommendation endpoints (Req 5.1-5.7).
/// Returns personalized recommendations based on the member's reading history.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class RecommendationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public RecommendationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets personalized book recommendations for the authenticated member (Req 5.1-5.7).
    /// Returns 1-10 AI-powered recommendations with justifications.
    /// Results are cached per member for 1 hour, invalidated on new loan or return.
    /// Members with fewer than 3 completed loans receive popular book suggestions.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>List of personalized book recommendations.</returns>
    /// <response code="200">Recommendations returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Member role.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetRecommendations(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var query = new GetRecommendationsQuery { UserId = userId.Value };
        var recommendations = await _mediator.Send(query, ct);

        return Ok(new { data = recommendations });
    }
}
