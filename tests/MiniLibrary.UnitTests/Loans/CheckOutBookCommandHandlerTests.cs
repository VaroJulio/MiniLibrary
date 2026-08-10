using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Loans.Commands.CheckOutBook;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.UnitTests.Loans;

public class CheckOutBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IWishlistRepository> _wishlistRepositoryMock;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CheckOutBookCommandHandler _handler;

    public CheckOutBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _wishlistRepositoryMock = new Mock<IWishlistRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CheckOutBookCommandHandler(
            _bookRepositoryMock.Object,
            _loanRepositoryMock.Object,
            _wishlistRepositoryMock.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidCheckOut_CreatesLoanAndUpdatesBookStatus()
    {
        // Arrange
        var book = Book.Create("Clean Code", "Robert Martin", "9780132350884", 2008, "Desc", "Software");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _wishlistRepositoryMock.Setup(r => r.GetEntryAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WishlistEntry?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BookId.Should().Be(book.Id);
        result.BookTitle.Should().Be("Clean Code");
        result.ReturnedAt.Should().BeNull();
        result.DueDate.Should().BeCloseTo(result.BorrowedAt.AddDays(14), TimeSpan.FromSeconds(1));

        _bookRepositoryMock.Verify(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()), Times.Once);
        _loanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BookLoan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new CheckOutBookCommand(Guid.NewGuid(), Guid.NewGuid());

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(command.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_BookNotAvailable_ThrowsConflictException()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        book.CheckOut(); // Set to CheckedOut
        var command = new CheckOutBookCommand(book.Id, Guid.NewGuid());

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*not available*");
    }

    [Fact]
    public async Task Handle_UserAtLoanLimit_ThrowsConflictException()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*maximum limit*5*");
    }

    [Fact]
    public async Task Handle_UserWith4ActiveLoans_Succeeds()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(4);
        _wishlistRepositoryMock.Setup(r => r.GetEntryAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WishlistEntry?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _loanRepositoryMock.Verify(r => r.AddAsync(It.IsAny<BookLoan>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConcurrencyConflict_ThrowsConflictException()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _mockUnitOfWork.Setup(u => u.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException("Concurrency conflict"));

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*checked out by another user*");
    }

    [Fact]
    public async Task Handle_BookInWishlist_RemovesFromWishlist()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);
        var wishlistEntry = WishlistEntry.Create(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _wishlistRepositoryMock.Setup(r => r.GetEntryAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(wishlistEntry);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _wishlistRepositoryMock.Verify(r => r.DeleteAsync(wishlistEntry, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotInWishlist_DoesNotCallDelete()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _wishlistRepositoryMock.Setup(r => r.GetEntryAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WishlistEntry?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _wishlistRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<WishlistEntry>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCheckOut_SetsCorrectLoanDuration()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckOutBookCommand(book.Id, userId);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanCountAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _wishlistRepositoryMock.Setup(r => r.GetEntryAsync(userId, book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((WishlistEntry?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        var expectedDuration = TimeSpan.FromDays(14);
        var actualDuration = result.DueDate - result.BorrowedAt;
        actualDuration.Should().Be(expectedDuration);
    }
}
