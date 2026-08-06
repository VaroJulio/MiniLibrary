using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Search endpoints (text and semantic). Accessible by all authenticated users.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class SearchController : ControllerBase
{
    /// <summary>
    /// Searches books by text query with optional filters.
    /// </summary>
    [HttpGet("books")]
    public IActionResult SearchBooks()
    {
        // Implementation in Task 8.2
        return Ok();
    }

    /// <summary>
    /// Performs semantic search using natural language.
    /// </summary>
    [HttpGet("semantic")]
    public IActionResult SemanticSearch()
    {
        // Implementation in Task 9.2
        return Ok();
    }
}
