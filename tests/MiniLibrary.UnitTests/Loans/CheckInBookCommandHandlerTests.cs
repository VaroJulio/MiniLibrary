using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Loans.Commands.CheckInBook;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.UnitTests.Loans;

public class CheckInBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly Mock<ILoanRepository> _loanRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CheckInBookCommandHandler _handler;

    public CheckInBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        _loanRepositoryMock = new Mock<ILoanRepository>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CheckInBookCommandHandler(
            _bookRepositoryMock.Object,
            _loanRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_MemberReturnsOwnBook_SetsReturnedAtAndMakesBookAvailable()
    {
        // Arrange
        var book = Book.Create("Clean Code", "Robert Martin", "9780132350884", 2008, "Desc", "Software");
        book.CheckOut();
        var userId = Guid.NewGuid();
        var loan = BookLoan.Create(book.Id, userId, DateTime.UtcNow.AddDays(-7));
        var command = new CheckInBookCommand(book.Id, userId);

        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Member);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanAsync(book.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.BookId.Should().Be(book.Id);
        result.BookTitle.Should().Be("Clean Code");
        result.ReturnedAt.Should().NotBeNull();
        book.Status.Should().Be(BookStatus.Available);

        _loanRepositoryMock.Verify(r => r.UpdateAsync(loan, It.IsAny<CancellationToken>()), Times.Once);
        _bookRepositoryMock.Verify(r => r.UpdateAsync(book, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new CheckInBookCommand(Guid.NewGuid(), Guid.NewGuid());
        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Member);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(command.BookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_MemberTriesToReturnOtherUsersLoan_ThrowsUnauthorizedAccessException()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        book.CheckOut();
        var loanOwnerUserId = Guid.NewGuid();
        var requestingUserId = Guid.NewGuid();
        var activeLoan = BookLoan.Create(book.Id, loanOwnerUserId, DateTime.UtcNow.AddDays(-5));
        var command = new CheckInBookCommand(book.Id, requestingUserId);

        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Member);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanAsync(book.Id, requestingUserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookLoan?)null);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(activeLoan);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*only check in books that you have borrowed*");
    }

    [Fact]
    public async Task Handle_LibrarianReturnsAnyUsersBook_Succeeds()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        book.CheckOut();
        var loanOwnerUserId = Guid.NewGuid();
        var librarianUserId = Guid.NewGuid();
        var loan = BookLoan.Create(book.Id, loanOwnerUserId, DateTime.UtcNow.AddDays(-3));
        var command = new CheckInBookCommand(book.Id, librarianUserId);

        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Librarian);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReturnedAt.Should().NotBeNull();
        book.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task Handle_AdminReturnsAnyUsersBook_Succeeds()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        book.CheckOut();
        var loanOwnerUserId = Guid.NewGuid();
        var adminUserId = Guid.NewGuid();
        var loan = BookLoan.Create(book.Id, loanOwnerUserId, DateTime.UtcNow.AddDays(-3));
        var command = new CheckInBookCommand(book.Id, adminUserId);

        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Admin);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ReturnedAt.Should().NotBeNull();
        book.Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task Handle_NoActiveLoanExists_ThrowsConflictException()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var userId = Guid.NewGuid();
        var command = new CheckInBookCommand(book.Id, userId);

        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Member);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanAsync(book.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookLoan?)null);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanByBookAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BookLoan?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*does not have an active loan*");
    }

    [Fact]
    public async Task Handle_SuccessfulCheckIn_RaisesBookReturnedEvent()
    {
        // Arrange
        var book = Book.Create("Test Book", "Author", "9780134685991", 2020, "Desc", "Fiction");
        book.CheckOut();
        var userId = Guid.NewGuid();
        var loan = BookLoan.Create(book.Id, userId, DateTime.UtcNow.AddDays(-7));
        var command = new CheckInBookCommand(book.Id, userId);

        _currentUserServiceMock.Setup(s => s.Role).Returns(UserRole.Member);
        _bookRepositoryMock.Setup(r => r.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);
        _loanRepositoryMock.Setup(r => r.GetActiveLoanAsync(book.Id, userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(loan);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert — BookReturnedEvent should be raised on the loan entity
        loan.DomainEvents.Should().ContainSingle(e => e is MiniLibrary.Domain.Events.BookReturnedEvent);
    }
}
