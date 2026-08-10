using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Wishlist.Commands.AddToWishlist;

/// <summary>
/// Handles AddToWishlistCommand: validates max 20 entries and no duplicates.
/// </summary>
public sealed class AddToWishlistCommandHandler : IRequestHandler<AddToWishlistCommand, Unit>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IBookRepository _bookRepository;
    private readonly IUnitOfWork _unitOfWork;
    private const int MaxWishlistSize = 20;

    public AddToWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IBookRepository bookRepository,
        IUnitOfWork unitOfWork)
    {
        _wishlistRepository = wishlistRepository;
        _bookRepository = bookRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AddToWishlistCommand request, CancellationToken cancellationToken)
    {
        // Verify book exists
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
            throw new NotFoundException("Book", request.BookId);

        // Check for duplicate
        var existing = await _wishlistRepository.GetEntryAsync(request.UserId, request.BookId, cancellationToken);
        if (existing is not null)
            throw new ConflictException("This book is already in your wishlist.");

        // Check max size
        var count = await _wishlistRepository.GetUserWishlistCountAsync(request.UserId, cancellationToken);
        if (count >= MaxWishlistSize)
            throw new ConflictException($"Wishlist is full. Maximum {MaxWishlistSize} entries allowed.");

        var entry = WishlistEntry.Create(request.BookId, request.UserId);
        await _wishlistRepository.AddAsync(entry, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
