using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Dashboard and statistics endpoints. Restricted to Librarian and Admin roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
public class DashboardController : ControllerBase
{
    /// <summary>
    /// Gets overview statistics: total books, available, checked out, active loans, users by role.
    /// </summary>
    [HttpGet("stats")]
    public IActionResult GetStats()
    {
        // Implementation in Task 12.2
        return Ok();
    }

    /// <summary>
    /// Gets loan metrics: loans by period, popular categories, top borrowed books.
    /// </summary>
    [HttpGet("loan-metrics")]
    public IActionResult GetLoanMetrics()
    {
        // Implementation in Task 12.2
        return Ok();
    }
}
