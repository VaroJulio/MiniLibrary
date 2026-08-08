using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.Application.Books.Commands.CreateBook;
using MiniLibrary.Application.Books.Commands.DeleteBook;
using MiniLibrary.Application.Books.Commands.UpdateBook;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Queries.GetBookById;

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
    private readonly IMediator _mediator;

    public BooksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Gets a book by its ID.
    /// </summary>
    /// <param name="id">Book identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="200">Book found.</response>
    /// <response code="404">Book not found.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct = default)
    {
        var result = await _mediator.Send(new GetBookByIdQuery(id), ct);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Creates a new book in the catalog.
    /// Requires Librarian or Admin role.
    /// </summary>
    /// <response code="201">Book created.</response>
    /// <response code="400">Validation error.</response>
    [HttpPost]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookCommand command, CancellationToken ct = default)
    {
        var result = await _mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Updates an existing book.
    /// Requires Librarian or Admin role.
    /// </summary>
    /// <param name="id">Book identifier</param>
    /// <param name="command">Update command with book data</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="200">Book updated.</response>
    /// <response code="404">Book not found.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    [ProducesResponseType(typeof(BookResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookCommand command, CancellationToken ct = default)
    {
        command = command with { Id = id };
        var result = await _mediator.Send(command, ct);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Deletes a book from the catalog (soft-delete).
    /// Requires Librarian or Admin role.
    /// </summary>
    /// <param name="id">Book identifier</param>
    /// <param name="ct">Cancellation token</param>
    /// <response code="204">Book deleted.</response>
    /// <response code="404">Book not found.</response>
    /// <response code="409">Book has active loans.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await _mediator.Send(new DeleteBookCommand(id), ct);
        return NoContent();
    }
}
