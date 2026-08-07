using MediatR;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Queries.GetBookRatings;

/// <summary>
/// Handles GetBookRatingsQuery by returning paginated ratings with author names.
/// </summary>
public sealed class GetBookRatingsQueryHandler
    : IRequestHandler<GetBookRatingsQuery, PagedResult<RatingResponse>>
{
    private readonly IRatingRepository _ratingRepository;

    public GetBookRatingsQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<PagedResult<RatingResponse>> Handle(
        GetBookRatingsQuery request,
        CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);
        var result = await _ratingRepository.GetBookRatingsAsync(request.BookId, paging, cancellationToken);

        var responses = result.Items.Select(r => new RatingResponse(
            r.Id,
            r.BookId,
            r.UserId,
            r.User?.FullName ?? "Unknown",
            r.Score,
            r.ReviewText,
            r.UsefulVotes,
            r.CreatedAt,
            r.UpdatedAt)).ToList();

        return new PagedResult<RatingResponse>(
            responses,
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}
