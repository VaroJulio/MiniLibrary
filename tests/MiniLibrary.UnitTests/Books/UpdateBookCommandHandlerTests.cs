using AutoMapper;
using MiniLibrary.Application.Books.Commands.UpdateBook;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Mappings;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.UnitTests.Books;

public class UpdateBookCommandHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly IMapper _mapper;
    private readonly UpdateBookCommandHandler _handler;

    public UpdateBookCommandHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BookMappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new UpdateBookCommandHandler(_bookRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_BookExists_UpdatesAndReturnsBookResponse()
    {
        // Arrange
        var existingBook = Book.Create("Old Title", "Old Author", "9780134685991", 2020, "Old Desc", "Fiction");
        var command = new UpdateBookCommand(
            existingBook.Id,
            "New Title",
            "New Author",
            "9780134685991", // Same ISBN
            2021,
            "New Description",
            "Science");

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(existingBook.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Title");
        result.Author.Should().Be("New Author");
        result.Isbn.Should().Be("9780134685991");
        result.PublishedYear.Should().Be(2021);
        result.Description.Should().Be("New Description");
        result.Category.Should().Be("Science");

        _bookRepositoryMock.Verify(r => r.UpdateAsync(existingBook, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_BookNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateBookCommand(
            Guid.NewGuid(),
            "Title",
            "Author",
            "9780134685991",
            2020,
            "Desc",
            "Fiction");

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_IsbnChangedAndDuplicate_ThrowsConflictException()
    {
        // Arrange
        var existingBook = Book.Create("Title", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var otherBook = Book.Create("Other", "Other Author", "9780201633610", 2019, "Other Desc", "Science");

        var command = new UpdateBookCommand(
            existingBook.Id,
            "Title",
            "Author",
            "9780201633610", // ISBN of the other book
            2020,
            "Desc",
            "Fiction");

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(existingBook.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);
        _bookRepositoryMock.Setup(r => r.GetByIsbnAsync("9780201633610", It.IsAny<CancellationToken>()))
            .ReturnsAsync(otherBook);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage("*ISBN*already exists*");
    }

    [Fact]
    public async Task Handle_IsbnUnchanged_DoesNotCheckUniqueness()
    {
        // Arrange
        var existingBook = Book.Create("Title", "Author", "9780134685991", 2020, "Desc", "Fiction");
        var command = new UpdateBookCommand(
            existingBook.Id,
            "New Title",
            "Author",
            "9780134685991", // Same ISBN
            2020,
            "Desc",
            "Fiction");

        _bookRepositoryMock.Setup(r => r.GetByIdAsync(existingBook.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.GetByIsbnAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
