using MediatR;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Ratings.Queries.GetBookRatings;

/// <summary>
/// Query to retrieve paginated ratings for a specific book.
/// </summary>
public sealed record GetBookRatingsQuery : IRequest<PagedResult<RatingResponse>>
{
    public Guid BookId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
