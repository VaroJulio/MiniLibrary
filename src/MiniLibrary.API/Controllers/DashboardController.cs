using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Dashboard.Queries.GetDashboardStats;
using MiniLibrary.Application.Dashboard.Queries.GetLoanMetrics;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Dashboard and statistics endpoints (Req 8.1-8.5).
/// Restricted to Librarian and Admin roles. Members receive 403.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets overview statistics: total books, available, checked out, active loans, users by role (Req 8.1-8.2).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Dashboard overview statistics.</returns>
    /// <response code="200">Stats returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Librarian or Admin role.</response>
    [HttpGet("stats")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetStats(CancellationToken ct = default)
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery(), ct);
        return Ok(new { data = stats });
    }

    /// <summary>
    /// Gets loan metrics: loans by period (7d, 30d, 12m), popular categories, top 10 most-borrowed books (Req 8.3-8.4).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Loan metrics data.</returns>
    /// <response code="200">Loan metrics returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Librarian or Admin role.</response>
    [HttpGet("loan-metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetLoanMetrics(CancellationToken ct = default)
    {
        var metrics = await _mediator.Send(new GetLoanMetricsQuery(), ct);
        return Ok(new { data = metrics });
    }
}
