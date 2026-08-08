using MediatR;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Ratings.Queries.GetRecentRatings;

/// <summary>
/// Query to retrieve recent community ratings across all books, paginated.
/// </summary>
public sealed record GetRecentRatingsQuery : IRequest<PagedResult<CommunityRatingResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
