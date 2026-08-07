using MediatR;

namespace MiniLibrary.Application.Wishlist.Commands.AddToWishlist;

/// <summary>
/// Command to add a book to the member's wishlist. Max 20 entries.
/// Rejects duplicates (409) and over-limit (409).
/// </summary>
public sealed record AddToWishlistCommand(Guid BookId, Guid UserId) : IRequest<Unit>;
