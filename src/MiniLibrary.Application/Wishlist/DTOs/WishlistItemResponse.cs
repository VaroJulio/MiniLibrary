namespace MiniLibrary.Application.Wishlist.DTOs;

/// <summary>
/// DTO representing a wishlist item in API responses.
/// </summary>
public sealed record WishlistItemResponse(
    Guid BookId,
    string Title,
    string Author,
    string BookStatus,
    DateTime AddedAt);

/// <summary>
/// DTO for add-to-wishlist request body.
/// </summary>
public sealed record AddToWishlistRequest(Guid BookId);
