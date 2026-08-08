using MediatR;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Ratings.Queries.GetMyRatings;

/// <summary>
/// Query to retrieve the current user's ratings across all books, paginated.
/// </summary>
public sealed record GetMyRatingsQuery : IRequest<PagedResult<MyRatingResponse>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
