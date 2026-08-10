using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Wishlist.Commands.AddToWishlist;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using Moq;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for wishlist size limit.
/// Property 14: Wishlist Size Limit — Generate random wishlist operations and verify
/// total entries never exceed 20, additions at limit rejected with 409.
/// **Validates: Requirements 18.8**
/// </summary>
[Trait("Category", "Property")]
public class WishlistSizeLimitProperties
{
    private const int MaxWishlistSize = 20;

    // ── Property 14a: Additions at limit are rejected ────────────────────────────

    /// <summary>
    /// When a wishlist has exactly 20 entries, adding another throws ConflictException.
    /// **Validates: Requirements 18.8**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property AddAtLimit_ThrowsConflict()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(MaxWishlistSize, MaxWishlistSize + 10)),
            currentCount =>
            {
                var userId = Guid.NewGuid();
                var bookId = Guid.NewGuid();

                var mockWishlist = new Mock<IWishlistRepository>();
                var mockBook = new Mock<IBookRepository>();

                mockBook.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Book.Create("Test", "Author", "9780306406157", 2020, "Desc", "Fiction"));

                mockWishlist.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WishlistEntry?)null);

                mockWishlist.Setup(r => r.GetUserWishlistCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(currentCount);

                var handler = new AddToWishlistCommandHandler(mockWishlist.Object, mockBook.Object, new Mock<IUnitOfWork>().Object);
                var command = new AddToWishlistCommand(bookId, userId);

                try
                {
                    handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
                    return false; // Should have thrown
                }
                catch (ConflictException)
                {
                    return true; // Expected: limit reached
                }
            });
    }

    // ── Property 14b: Additions below limit succeed ──────────────────────────────

    /// <summary>
    /// When a wishlist has fewer than 20 entries, adding a new book succeeds.
    /// **Validates: Requirements 18.8**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property AddBelowLimit_Succeeds()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, MaxWishlistSize - 1)),
            currentCount =>
            {
                var userId = Guid.NewGuid();
                var bookId = Guid.NewGuid();

                var mockWishlist = new Mock<IWishlistRepository>();
                var mockBook = new Mock<IBookRepository>();

                mockBook.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Book.Create("Test", "Author", "9780306406157", 2020, "Desc", "Fiction"));

                mockWishlist.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WishlistEntry?)null);

                mockWishlist.Setup(r => r.GetUserWishlistCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(currentCount);

                mockWishlist.Setup(r => r.AddAsync(It.IsAny<WishlistEntry>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var handler = new AddToWishlistCommandHandler(mockWishlist.Object, mockBook.Object, new Mock<IUnitOfWork>().Object);
                var command = new AddToWishlistCommand(bookId, userId);

                try
                {
                    handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
                    return true; // Success expected
                }
                catch
                {
                    return false; // Should not throw
                }
            });
    }

    // ── Property 14c: Duplicate additions are rejected ───────────────────────────

    /// <summary>
    /// Adding a book that already exists in the wishlist throws ConflictException.
    /// **Validates: Requirements 18.8**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property AddDuplicate_ThrowsConflict()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, MaxWishlistSize - 1)),
            currentCount =>
            {
                var userId = Guid.NewGuid();
                var bookId = Guid.NewGuid();

                var mockWishlist = new Mock<IWishlistRepository>();
                var mockBook = new Mock<IBookRepository>();

                mockBook.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(Book.Create("Test", "Author", "9780306406157", 2020, "Desc", "Fiction"));

                // Entry already exists
                mockWishlist.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(WishlistEntry.Create(bookId, userId));

                var handler = new AddToWishlistCommandHandler(mockWishlist.Object, mockBook.Object, new Mock<IUnitOfWork>().Object);
                var command = new AddToWishlistCommand(bookId, userId);

                try
                {
                    handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
                    return false; // Should have thrown
                }
                catch (ConflictException)
                {
                    return true; // Expected: duplicate
                }
            });
    }

    // ── Property 14d: Size never exceeds 20 after any sequence of operations ────

    /// <summary>
    /// For any sequence of N add attempts (N > 20), at most 20 succeed.
    /// **Validates: Requirements 18.8**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property SequenceOfAdds_NeverExceedsLimit()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(15, 30)),
            totalAttempts =>
            {
                var userId = Guid.NewGuid();
                var successCount = 0;
                var currentCount = 0;

                var mockWishlist = new Mock<IWishlistRepository>();
                var mockBook = new Mock<IBookRepository>();

                for (int i = 0; i < totalAttempts; i++)
                {
                    var bookId = Guid.NewGuid();

                    mockBook.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(Book.Create("Book" + i, "Author", "9780306406157", 2020, "Desc", "Fiction"));

                    mockWishlist.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync((WishlistEntry?)null);

                    var capturedCount = currentCount;
                    mockWishlist.Setup(r => r.GetUserWishlistCountAsync(userId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(capturedCount);

                    mockWishlist.Setup(r => r.AddAsync(It.IsAny<WishlistEntry>(), It.IsAny<CancellationToken>()))
                        .Returns(Task.CompletedTask);

                    var handler = new AddToWishlistCommandHandler(mockWishlist.Object, mockBook.Object, new Mock<IUnitOfWork>().Object);
                    var command = new AddToWishlistCommand(bookId, userId);

                    try
                    {
                        handler.Handle(command, CancellationToken.None).GetAwaiter().GetResult();
                        successCount++;
                        currentCount++;
                    }
                    catch (ConflictException)
                    {
                        // Expected when at limit
                    }
                }

                return successCount <= MaxWishlistSize;
            });
    }
}
