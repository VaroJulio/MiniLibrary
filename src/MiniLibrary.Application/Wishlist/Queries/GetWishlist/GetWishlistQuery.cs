using MediatR;
using MiniLibrary.Application.Wishlist.DTOs;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Wishlist.Queries.GetWishlist;

/// <summary>
/// Query to retrieve the member's wishlist (paginated, 20/page).
/// </summary>
public sealed record GetWishlistQuery : IRequest<PagedResult<WishlistItemResponse>>
{
    public Guid UserId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
