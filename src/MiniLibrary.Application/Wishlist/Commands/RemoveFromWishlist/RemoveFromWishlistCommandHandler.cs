using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Wishlist.Commands.RemoveFromWishlist;

/// <summary>
/// Handles RemoveFromWishlistCommand: removes the entry or throws 404.
/// </summary>
public sealed class RemoveFromWishlistCommandHandler : IRequestHandler<RemoveFromWishlistCommand, Unit>
{
    private readonly IWishlistRepository _wishlistRepository;

    public RemoveFromWishlistCommandHandler(IWishlistRepository wishlistRepository)
    {
        _wishlistRepository = wishlistRepository;
    }

    public async Task<Unit> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var entry = await _wishlistRepository.GetEntryAsync(request.UserId, request.BookId, cancellationToken);
        if (entry is null)
            throw new NotFoundException("WishlistEntry", $"user={request.UserId}, book={request.BookId}");

        await _wishlistRepository.DeleteAsync(entry, cancellationToken);
        return Unit.Value;
    }
}
