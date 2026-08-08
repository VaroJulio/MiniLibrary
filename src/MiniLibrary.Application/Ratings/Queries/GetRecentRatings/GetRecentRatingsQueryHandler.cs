using MediatR;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Queries.GetRecentRatings;

/// <summary>
/// Handles GetRecentRatingsQuery by returning recent community ratings with book and user info.
/// </summary>
public sealed class GetRecentRatingsQueryHandler
    : IRequestHandler<GetRecentRatingsQuery, PagedResult<CommunityRatingResponse>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetRecentRatingsQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<PagedResult<CommunityRatingResponse>> Handle(
        GetRecentRatingsQuery request,
        CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);
        var result = await _ratingRepository.GetRecentRatingsAsync(paging, cancellationToken);

        var responses = result.Items.Select(r => new CommunityRatingResponse(
            r.Id,
            r.BookId,
            r.Book?.Title ?? "Unknown",
            r.Book?.Author ?? "Unknown",
            r.UserId,
            r.User?.FullName ?? "Unknown",
            r.Score,
            r.ReviewText,
            r.UsefulVotes,
            r.CreatedAt)).ToList();

        return new PagedResult<CommunityRatingResponse>(
            responses,
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}
