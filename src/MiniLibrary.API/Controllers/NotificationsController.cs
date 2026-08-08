using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Notifications.Commands.MarkNotificationRead;
using MiniLibrary.Application.Notifications.Queries.GetNotifications;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Notification endpoints (Req 18.4-18.5). Members can view and manage their notifications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;

    public NotificationsController(IMediator mediator, ICurrentUserService currentUserService)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }

    /// <summary>
    /// Gets the current user's notifications (max 50, ordered by date desc, read + unread).
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Items per page (default: 50, max: 50).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Notifications returned successfully.</response>
    /// <response code="401">User is not authenticated.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Unauthorized();

        var query = new GetNotificationsQuery { UserId = userId.Value, Page = page, PageSize = pageSize };
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
    /// Marks a notification as read (Req 18.5).
    /// </summary>
    /// <param name="id">Notification identifier.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Notification marked as read.</response>
    /// <response code="401">User is not authenticated.</response>
    /// <response code="404">Notification not found or not owned by user.</response>
    [HttpPut("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(Guid id, CancellationToken ct = default)
    {
        var userId = _currentUserService.UserId;
        if (userId is null) return Unauthorized();

        var command = new MarkNotificationReadCommand(id, userId.Value);
        await _mediator.Send(command, ct);

        return NoContent();
    }
}
