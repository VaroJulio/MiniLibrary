using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Notification endpoints. Members can view and manage their notifications.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class NotificationsController : ControllerBase
{
    /// <summary>
    /// Gets the current user's notifications (max 50, ordered by date desc).
    /// </summary>
    [HttpGet]
    public IActionResult GetNotifications()
    {
        // Implementation in Task 15.2
        return Ok();
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    /// <param name="id">Notification identifier</param>
    [HttpPut("{id:guid}/read")]
    public IActionResult MarkAsRead(Guid id)
    {
        // Implementation in Task 15.2
        return Ok();
    }
}
