using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Loans.Commands.CheckOutBook;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using Moq;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for loan preconditions and correctness.
/// **Validates: Requirements 2.3, 2.6, 13.4**
/// </summary>
public class LoanPreconditionProperties
{
    // ── Property 3: Check-Out Preconditions ──────────────────────────────────────
    // Generate random (bookStatus, activeLoanCount) pairs and verify check-out
    // succeeds iff book status == Available AND activeLoanCount < 5.

    /// <summary>
    /// **Validates: Requirements 2.3, 2.6**
    /// Check-out succeeds when book is Available AND user has fewer than 5 active loans.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property CheckOut_Succeeds_WhenBookAvailableAndUnderLoanLimit()
    {
        return Prop.ForAll(
            Arb.From(GenActiveLoanCountUnderLimit()),
            activeLoanCount =>
            {
                // Arrange: book is Available, user has < 5 active loans
                var bookId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);
                mockBookRepo.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeLoanCount);
                mockLoanRepo.Setup(r => r.AddAsync(It.IsAny<BookLoan>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockWishlistRepo = new Mock<IWishlistRepository>();
                mockWishlistRepo.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WishlistEntry?)null);

                var handler = new CheckOutBookCommandHandler(
                    mockBookRepo.Object, mockLoanRepo.Object, mockWishlistRepo.Object);

                // Act
                var result = handler.Handle(new CheckOutBookCommand(bookId, userId), CancellationToken.None)
                    .GetAwaiter().GetResult();

                // Assert: operation succeeded and book status changed
                return result is not null && book.Status == BookStatus.CheckedOut;
            });
    }

    /// <summary>
    /// **Validates: Requirements 2.3**
    /// Check-out fails with ConflictException when book is not Available (CheckedOut).
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property CheckOut_Fails_WhenBookIsCheckedOut()
    {
        return Prop.ForAll(
            Arb.From(GenActiveLoanCountAny()),
            activeLoanCount =>
            {
                // Arrange: book is CheckedOut
                var bookId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");
                book.CheckOut(); // Set status to CheckedOut

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeLoanCount);

                var mockWishlistRepo = new Mock<IWishlistRepository>();

                var handler = new CheckOutBookCommandHandler(
                    mockBookRepo.Object, mockLoanRepo.Object, mockWishlistRepo.Object);

                // Act & Assert: should throw ConflictException
                try
                {
                    handler.Handle(new CheckOutBookCommand(bookId, userId), CancellationToken.None)
                        .GetAwaiter().GetResult();
                    return false; // Should have thrown
                }
                catch (ConflictException)
                {
                    return true; // Expected: book not available
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 2.6**
    /// Check-out fails with ConflictException when user has 5 or more active loans,
    /// even if the book is Available.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property CheckOut_Fails_WhenUserAtOrOverLoanLimit()
    {
        return Prop.ForAll(
            Arb.From(GenActiveLoanCountAtOrOverLimit()),
            activeLoanCount =>
            {
                // Arrange: book is Available but user has >= 5 active loans
                var bookId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeLoanCount);

                var mockWishlistRepo = new Mock<IWishlistRepository>();

                var handler = new CheckOutBookCommandHandler(
                    mockBookRepo.Object, mockLoanRepo.Object, mockWishlistRepo.Object);

                // Act & Assert: should throw ConflictException
                try
                {
                    handler.Handle(new CheckOutBookCommand(bookId, userId), CancellationToken.None)
                        .GetAwaiter().GetResult();
                    return false; // Should have thrown
                }
                catch (ConflictException)
                {
                    return true; // Expected: loan limit reached
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 2.3, 2.6**
    /// For any random (bookStatus, activeLoanCount) pair, check-out succeeds
    /// if and only if status == Available AND count < 5.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property CheckOut_SucceedsIffAvailableAndUnderLimit()
    {
        return Prop.ForAll(
            Arb.From(GenBookStatusAndLoanCount()),
            pair =>
            {
                var (isAvailable, activeLoanCount) = pair;

                var bookId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");
                if (!isAvailable)
                    book.CheckOut(); // Set to CheckedOut

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);
                mockBookRepo.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeLoanCount);
                mockLoanRepo.Setup(r => r.AddAsync(It.IsAny<BookLoan>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockWishlistRepo = new Mock<IWishlistRepository>();
                mockWishlistRepo.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WishlistEntry?)null);

                var handler = new CheckOutBookCommandHandler(
                    mockBookRepo.Object, mockLoanRepo.Object, mockWishlistRepo.Object);

                var shouldSucceed = isAvailable && activeLoanCount < 5;

                try
                {
                    var result = handler.Handle(new CheckOutBookCommand(bookId, userId), CancellationToken.None)
                        .GetAwaiter().GetResult();
                    // If we get here, the operation succeeded
                    return shouldSucceed;
                }
                catch (ConflictException)
                {
                    // If we get here, the operation was rejected
                    return !shouldSucceed;
                }
            });
    }

    // ── Property 4: Loan Creation Correctness ────────────────────────────────────
    // Generate random check-out events and verify DueDate == BorrowedAt + 14 days.

    /// <summary>
    /// **Validates: Requirements 2.3, 2.6, 13.4**
    /// For any successful check-out, DueDate == BorrowedAt + 14 days.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Loan_DueDate_Is_BorrowedAt_Plus14Days()
    {
        return Prop.ForAll(
            Arb.From(GenBorrowedAtDate()),
            borrowedAt =>
            {
                // Test the domain entity directly: BookLoan.Create sets DueDate correctly
                var bookId = Guid.NewGuid();
                var userId = Guid.NewGuid();

                var loan = BookLoan.Create(bookId, userId, borrowedAt);

                var expectedDueDate = borrowedAt.AddDays(14);

                return loan.DueDate == expectedDueDate
                    && loan.BorrowedAt == borrowedAt
                    && loan.BookId == bookId
                    && loan.UserId == userId
                    && loan.ReturnedAt == null
                    && loan.IsActive;
            });
    }

    /// <summary>
    /// **Validates: Requirements 2.3, 2.6, 13.4**
    /// For any successful check-out via the handler, the returned DueDate == BorrowedAt + 14 days.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property CheckOut_Handler_ReturnsDueDateAs_BorrowedAtPlus14()
    {
        return Prop.ForAll(
            Arb.From(GenActiveLoanCountUnderLimit()),
            activeLoanCount =>
            {
                var bookId = Guid.NewGuid();
                var userId = Guid.NewGuid();
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);
                mockBookRepo.Setup(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeLoanCount);
                mockLoanRepo.Setup(r => r.AddAsync(It.IsAny<BookLoan>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockWishlistRepo = new Mock<IWishlistRepository>();
                mockWishlistRepo.Setup(r => r.GetEntryAsync(userId, bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((WishlistEntry?)null);

                var handler = new CheckOutBookCommandHandler(
                    mockBookRepo.Object, mockLoanRepo.Object, mockWishlistRepo.Object);

                var result = handler.Handle(new CheckOutBookCommand(bookId, userId), CancellationToken.None)
                    .GetAwaiter().GetResult();

                // The handler uses DateTime.UtcNow internally, so we verify the invariant:
                // DueDate == BorrowedAt + 14 days
                return result.DueDate == result.BorrowedAt.AddDays(14);
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Custom Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates active loan counts in [0, 4] (under the limit of 5).
    /// </summary>
    private static Gen<int> GenActiveLoanCountUnderLimit()
    {
        return Gen.Choose(0, 4);
    }

    /// <summary>
    /// Generates active loan counts in [5, 10] (at or over the limit).
    /// </summary>
    private static Gen<int> GenActiveLoanCountAtOrOverLimit()
    {
        return Gen.Choose(5, 10);
    }

    /// <summary>
    /// Generates active loan counts in [0, 10] (any valid count).
    /// </summary>
    private static Gen<int> GenActiveLoanCountAny()
    {
        return Gen.Choose(0, 10);
    }

    /// <summary>
    /// Generates random (isAvailable, activeLoanCount) pairs covering all
    /// combinations of book availability and loan count.
    /// </summary>
    private static Gen<(bool isAvailable, int activeLoanCount)> GenBookStatusAndLoanCount()
    {
        return from isAvailable in Gen.Elements(true, false)
               from loanCount in Gen.Choose(0, 10)
               select (isAvailable, loanCount);
    }

    /// <summary>
    /// Generates random DateTime values for BorrowedAt within a reasonable range.
    /// </summary>
    private static Gen<DateTime> GenBorrowedAtDate()
    {
        return Gen.Choose(0, 365 * 30) // Up to 30 years of days
            .Select(daysOffset =>
                new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddDays(daysOffset));
    }
}
