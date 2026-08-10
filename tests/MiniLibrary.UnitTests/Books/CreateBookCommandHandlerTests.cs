using AutoMapper;
using MiniLibrary.Application.Books.Commands.CreateBook;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Mappings;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.UnitTests.Books;

public class CreateBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly IMapper _mapper;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateBookCommandHandler _handler;

    public CreateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BookMappingProfile>());
        _mapper = config.CreateMapper();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateBookCommandHandler(_bookRepositoryMock.Object, _mapper, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesBookAndReturnsResponse()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Clean Code",
            "Robert C. Martin",
            "9780132350884",
            2008,
            "A Handbook of Agile Software Craftsmanship",
            "Software Engineering");

        _bookRepositoryMock.Setup(r => r.GetByIsbnAsync(command.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Clean Code");
        result.Author.Should().Be("Robert C. Martin");
        result.Isbn.Should().Be("9780132350884");
        result.PublishedYear.Should().Be(2008);
        result.Description.Should().Be("A Handbook of Agile Software Craftsmanship");
        result.Category.Should().Be("Software Engineering");
        result.Status.Should().Be("Available");
        result.AverageRating.Should().Be(0m);
        result.TotalRatings.Should().Be(0);

        _bookRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateIsbn_ThrowsConflictException()
    {
        // Arrange
        var existingBook = Book.Create("Existing", "Author", "9780132350884", 2008, "Desc", "Cat");
        var command = new CreateBookCommand(
            "New Book",
            "New Author",
            "9780132350884",
            2020,
            "New Description",
            "Fiction");

        _bookRepositoryMock.Setup(r => r.GetByIsbnAsync(command.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*ISBN*already exists*");

        _bookRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ValidCommand_SetsInitialStatusToAvailable()
    {
        // Arrange
        var command = new CreateBookCommand(
            "Test Book",
            "Test Author",
            "9780134685991",
            2020,
            "Test Description",
            "Fiction");

        Book? capturedBook = null;
        _bookRepositoryMock.Setup(r => r.GetByIsbnAsync(command.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);
        _bookRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, _) => capturedBook = book);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        capturedBook.Should().NotBeNull();
        capturedBook!.Status.Should().Be(Domain.Enumerations.BookStatus.Available);
    }
}
