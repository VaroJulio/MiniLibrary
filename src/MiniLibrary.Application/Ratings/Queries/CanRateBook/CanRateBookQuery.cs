using MediatR;

namespace MiniLibrary.Application.Ratings.Queries.CanRateBook;

/// <summary>
/// Query to check if the current user can rate a specific book
/// (has an unrated completed loan cycle).
/// </summary>
public sealed record CanRateBookQuery : IRequest<CanRateBookResponse>
{
    public Guid BookId { get; init; }
    public Guid UserId { get; init; }
}

/// <summary>
/// Response indicating whether the user can rate and which loan to associate.
/// </summary>
public sealed record CanRateBookResponse(bool CanRate, Guid? LoanId);
