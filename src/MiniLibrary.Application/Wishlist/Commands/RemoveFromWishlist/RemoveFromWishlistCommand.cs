using MediatR;

namespace MiniLibrary.Application.Wishlist.Commands.RemoveFromWishlist;

/// <summary>
/// Command to remove a book from the member's wishlist.
/// </summary>
public sealed record RemoveFromWishlistCommand(Guid BookId, Guid UserId) : IRequest<Unit>;
