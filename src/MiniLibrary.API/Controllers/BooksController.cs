using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.API.Controllers;

/// <summary>
/// Book catalog management endpoints.
/// Read access for all authenticated users; write access for Librarian and Admin only.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = AuthorizationConfig.Policies.Authenticated)]
public class BooksController : ControllerBase
{
    /// <summary>
    /// Gets a book by its ID.
    /// </summary>
    /// <param name="id">Book identifier</param>
    [HttpGet("{id:guid}")]
    public IActionResult GetById(Guid id)
    {
        // Implementation in Task 6.3
        return Ok();
    }

    /// <summary>
    /// Creates a new book in the catalog.
    /// Requires Librarian or Admin role.
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    public IActionResult Create()
    {
        // Implementation in Task 6.3
        return StatusCode(201);
    }

    /// <summary>
    /// Updates an existing book.
    /// Requires Librarian or Admin role.
    /// </summary>
    /// <param name="id">Book identifier</param>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    public IActionResult Update(Guid id)
    {
        // Implementation in Task 6.3
        return Ok();
    }

    /// <summary>
    /// Deletes a book from the catalog (soft-delete).
    /// Requires Librarian or Admin role.
    /// </summary>
    /// <param name="id">Book identifier</param>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    public IActionResult Delete(Guid id)
    {
        // Implementation in Task 6.3
        return NoContent();
    }
}
