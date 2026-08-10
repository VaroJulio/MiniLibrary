using MiniLibrary.Application.Books.Commands.DeleteBook;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.UnitTests.Books;

public class DeleteBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeleteBookCommandHandler _handler;

    public DeleteBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new DeleteBookCommandHandler(_bookRepositoryMock.Object, _loanRepositoryMock.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_BookExistsNoActiveLoans_SoftDeletesBook()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var command = new DeleteBookCommand(book.Id);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookLoan?)null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        book.IsDeleted.Should().BeTrue();
        _bookRepositoryMock.Verify(r => r.DeleteAsync(book, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeleteBookCommand(Guid.NewGuid());

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_BookHasActiveLoans_ThrowsConflictException()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var user = User.Create("user@test.com", "Test User", "ext-123", "Google");
        var activeLoan = BookLoan.Create(book.Id, user.Id, DateTime.UtcNow);
        var command = new DeleteBookCommand(book.Id);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeLoan);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*active loans*");
    }

    [Fact]
    public async Task Handle_BookHasActiveLoans_DoesNotDeleteBook()
    {
        // Arrange
        var book = Book.Create("Title", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var user = User.Create("user@test.com", "Test User", "ext-123", "Google");
        var activeLoan = BookLoan.Create(book.Id, user.Id, DateTime.UtcNow);
        var command = new DeleteBookCommand(book.Id);

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeLoan);

        // Act
        try { await _handler.Handle(command, CancellationToken.None); } catch { }

        // Assert
        book.IsDeleted.Should().BeFalse();
        _bookRepositoryMock.Verify(r => r.DeleteAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
