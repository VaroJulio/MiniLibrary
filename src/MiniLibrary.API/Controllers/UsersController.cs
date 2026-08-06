using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// User management endpoints. Admin-only access for user listing and role assignment.
/// Profile access for any authenticated user.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class UsersController : ControllerBase
{
    /// <summary>
    /// Lists all users (paginated). Requires Admin role.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationConfig.Policies.AdminOnly)]
    public IActionResult GetAll()
    {
        // Implementation in Task 12.1
        return Ok();
    }

    /// <summary>
    /// Assigns a role to a user. Requires Admin role.
    /// Prevents the sole Admin from changing their own role.
    /// </summary>
    /// <param name="id">User identifier</param>
    [HttpPut("{id:guid}/role")]
    [Authorize(Policy = AuthorizationConfig.Policies.AdminOnly)]
    public IActionResult AssignRole(Guid id)
    {
        // Implementation in Task 12.1
        return Ok();
    }

    /// <summary>
    /// Gets the current authenticated user's profile.
    /// </summary>
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        // Implementation in Task 12.1
        return Ok();
    }
}
