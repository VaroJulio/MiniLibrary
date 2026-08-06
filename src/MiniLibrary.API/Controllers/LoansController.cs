using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Loan management endpoints (check-out, check-in, history).
/// Members can manage their own loans; Librarians can manage any loan.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class LoansController : ControllerBase
{
    /// <summary>
    /// Checks out a book for the authenticated member.
    /// Requires Member, Librarian, or Admin role.
    /// </summary>
    [HttpPost("checkout")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    public IActionResult CheckOut()
    {
        // Implementation in Task 7.3
        return Ok();
    }

    /// <summary>
    /// Checks in a book.
    /// Members can return their own loans; Librarians/Admins can return any loan.
    /// </summary>
    [HttpPost("checkin")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    public IActionResult CheckIn()
    {
        // Implementation in Task 7.3
        return Ok();
    }

    /// <summary>
    /// Gets the current user's loan history (paginated).
    /// </summary>
    [HttpGet("history")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    public IActionResult GetHistory()
    {
        // Implementation in Task 7.3
        return Ok();
    }

    /// <summary>
    /// Gets all overdue loans. Requires Librarian or Admin role.
    /// </summary>
    [HttpGet("overdue")]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    public IActionResult GetOverdue()
    {
        // Implementation in Task 7.3
        return Ok();
    }
}
