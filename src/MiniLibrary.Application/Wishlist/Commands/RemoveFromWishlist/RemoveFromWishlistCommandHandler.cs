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
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFromWishlistCommandHandler(IWishlistRepository wishlistRepository, IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(RemoveFromWishlistCommand request, CancellationToken cancellationToken)
    {
        var entry = await _wishlistRepository.GetEntryAsync(request.UserId, request.BookId, cancellationToken);
        if (entry is null)
            throw new NotFoundException("WishlistEntry", $"user={request.UserId}, book={request.BookId}");

        await _wishlistRepository.DeleteAsync(entry, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);
        return Unit.Value;
    }
}
