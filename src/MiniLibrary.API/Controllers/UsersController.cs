using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Users.Commands.AssignRole;
using MiniLibrary.Application.Users.DTOs;
using MiniLibrary.Application.Users.Queries.GetProfile;
using MiniLibrary.Application.Users.Queries.GetUsers;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// User management endpoints (Req 7.1-7.5).
/// Admin-only access for user listing and role assignment.
/// Profile access for any authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public UsersController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Lists all users with pagination (Req 7.1). Requires Admin role.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 20).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Paginated list of users.</returns>
    /// <response code="200">User list returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Admin role.</response>
    [HttpGet]
    [Authorize(Policy = AuthorizationConfig.Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetUsersQuery { Page = page, PageSize = pageSize };
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
    /// Assigns a role to a user (Req 7.2-7.3). Requires Admin role.
    /// Prevents the sole Admin from changing their own role.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="request">The new role to assign.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Role assigned successfully.</response>
    /// <response code="400">Invalid role value.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="403">User does not have Admin role.</response>
    /// <response code="404">User not found.</response>
    /// <response code="409">Cannot change sole Admin's role.</response>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = AuthorizationConfig.Policies.AdminOnly)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignRole(
        Guid id,
        [FromBody] AssignRoleRequest request,
        CancellationToken ct = default)
    {
        if (!Enum.TryParse<UserRole>(request.Role, ignoreCase: true, out var role))
        {
            return BadRequest(new { error = $"Invalid role '{request.Role}'. Valid roles: Admin, Librarian, Member." });
        }

        var command = new AssignRoleCommand(id, role);
        await _mediator.Send(command, ct);

        return NoContent();
    }

    /// <summary>
    /// Gets the current authenticated user's profile (Req 7.4).
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>User profile details.</returns>
    /// <response code="200">Profile returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">User profile not found.</response>
    [HttpGet("profile")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile(CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null)
        {
            return Unauthorized();
        }

        var query = new GetProfileQuery(userId.Value);
        var profile = await _mediator.Send(query, ct);

        if (profile is null)
        {
            return NotFound(new { error = "User profile not found." });
        }

        return Ok(new { data = profile });
    }
}
