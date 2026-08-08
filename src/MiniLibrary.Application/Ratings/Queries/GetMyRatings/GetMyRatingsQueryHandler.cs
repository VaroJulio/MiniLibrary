using MediatR;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Queries.GetMyRatings;

/// <summary>
/// Handles GetMyRatingsQuery by returning the user's own ratings with book info.
/// </summary>
public sealed class GetMyRatingsQueryHandler
    : IRequestHandler<GetMyRatingsQuery, PagedResult<MyRatingResponse>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetMyRatingsQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<PagedResult<MyRatingResponse>> Handle(
        GetMyRatingsQuery request,
        CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);
        var result = await _ratingRepository.GetUserRatingsAsync(request.UserId, paging, cancellationToken);

        var responses = result.Items.Select(r => new MyRatingResponse(
            r.Id,
            r.BookId,
            r.Book?.Title ?? "Unknown",
            r.Book?.Author ?? "Unknown",
            r.Score,
            r.ReviewText,
            r.UsefulVotes,
            r.CreatedAt,
            r.UpdatedAt)).ToList();

        return new PagedResult<MyRatingResponse>(
            responses,
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}
