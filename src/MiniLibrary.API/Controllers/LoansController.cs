using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MiniLibrary.API.Configuration;
using MiniLibrary.API.Extensions;
using MiniLibrary.Application.Loans.Commands.CheckInBook;
using MiniLibrary.Application.Loans.Commands.CheckOutBook;
using MiniLibrary.Application.Loans.Queries.GetLoanHistory;
using MiniLibrary.Application.Loans.Queries.GetOverdueLoans;

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
    private readonly IMediator _mediator;

    public LoansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Checks out a book for the authenticated member.
    /// </summary>
    /// <response code="200">Book checked out successfully.</response>
    /// <response code="400">Precondition failed (book unavailable or loan limit reached).</response>
    /// <response code="409">Concurrency conflict.</response>
    [HttpPost("checkout")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CheckOut([FromBody] CheckOutRequest request, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CheckOutBookCommand(request.BookId, userId.Value);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Checks in a book.
    /// Members can return their own loans; Librarians/Admins can return any loan.
    /// </summary>
    /// <response code="200">Book returned successfully.</response>
    /// <response code="404">Loan not found.</response>
    [HttpPost("checkin")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckIn([FromBody] CheckInRequest request, CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var command = new CheckInBookCommand(request.BookId, userId.Value);
        var result = await _mediator.Send(command, ct);
        return Ok(result);
    }

    /// <summary>
    /// Gets the current user's loan history (paginated).
    /// </summary>
    /// <response code="200">Loan history.</response>
    [HttpGet("history")]
    [Authorize(Policy = AuthorizationConfig.Policies.MemberOnly)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = User.GetUserId();
        if (userId is null) return Unauthorized();

        var query = new GetLoanHistoryQuery(userId.Value, page, pageSize);
        var result = await _mediator.Send(query, ct);

        return this.ToPagedOk(result);
    }

    /// <summary>
    /// Gets all overdue loans. Requires Librarian or Admin role.
    /// </summary>
    /// <response code="200">Overdue loans list.</response>
    [HttpGet("overdue")]
    [Authorize(Policy = AuthorizationConfig.Policies.LibrarianOrAdmin)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOverdue(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var query = new GetOverdueLoansQuery(page, pageSize);
        var result = await _mediator.Send(query, ct);

        return this.ToPagedOk(result);
    }
}

/// <summary>Request body for check-out.</summary>
public record CheckOutRequest(Guid BookId);

/// <summary>Request body for check-in.</summary>
public record CheckInRequest(Guid BookId);
