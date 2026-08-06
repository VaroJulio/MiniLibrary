using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// AI-powered book recommendation endpoints. Accessible by all authenticated users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
public class RecommendationsController : ControllerBase
{
    /// <summary>
    /// Gets personalized book recommendations for the authenticated member.
    /// </summary>
    [HttpGet]
    public IActionResult GetRecommendations()
    {
        // Implementation in Task 11.2
        return Ok();
    }
}
