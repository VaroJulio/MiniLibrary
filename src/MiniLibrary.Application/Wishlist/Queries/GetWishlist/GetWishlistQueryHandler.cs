using MediatR;
using MiniLibrary.Application.Wishlist.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Wishlist.Queries.GetWishlist;

/// <summary>
/// Handles GetWishlistQuery: returns paginated wishlist with book status.
/// </summary>
public sealed class GetWishlistQueryHandler
    : IRequestHandler<GetWishlistQuery, PagedResult<WishlistItemResponse>>
{
    private readonly IWishlistRepository _wishlistRepository;

    public GetWishlistQueryHandler(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public async Task<PagedResult<WishlistItemResponse>> Handle(
        GetWishlistQuery request,
        CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);
        var result = await _wishlistRepository.GetUserWishlistAsync(request.UserId, paging, cancellationToken);

        var items = result.Items.Select(e => new WishlistItemResponse(
            e.BookId,
            e.Book?.Title ?? "Unknown",
            e.Book?.Author ?? "Unknown",
            e.Book?.Status.ToString() ?? "Unknown",
            e.AddedAt)).ToList();

        return new PagedResult<WishlistItemResponse>(
            items, result.TotalCount, result.Page, result.PageSize);
    }
}
