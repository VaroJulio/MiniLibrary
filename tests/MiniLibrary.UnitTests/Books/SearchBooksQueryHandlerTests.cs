using AutoMapper;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Mappings;
using MiniLibrary.Application.Books.Queries.SearchBooks;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.UnitTests.Books;

public class SearchBooksQueryHandlerTests
{
    private readonly Mock<IBookRepository> _bookRepositoryMock;
    private readonly IMapper _mapper;
    private readonly SearchBooksQueryHandler _handler;

    public SearchBooksQueryHandlerTests()
    {
        _bookRepositoryMock = new Mock<IBookRepository>();
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BookMappingProfile>());
        _mapper = config.CreateMapper();
        _handler = new SearchBooksQueryHandler(_bookRepositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_WithSearchTerm_DelegatesToRepositoryWithCorrectCriteria()
    {
        // Arrange
        var query = new SearchBooksQuery
        {
            SearchTerm = "Clean Code",
            Page = 1,
            PageSize = 20
        };

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<Book>.Empty(1, 20));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.SearchAsync(
            It.Is<SearchCriteria>(c =>
                c.Query == "Clean Code" &&
                c.Page == 1 &&
                c.PageSize == 20),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithFilters_PassesAllFiltersToSearchCriteria()
    {
        // Arrange
        var query = new SearchBooksQuery
        {
            SearchTerm = "test",
            Category = "Fiction",
            Status = BookStatus.Available,
            YearFrom = 2000,
            YearTo = 2023,
            SortBy = "title",
            SortDescending = true,
            Page = 2,
            PageSize = 10
        };

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<Book>.Empty(2, 10));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.SearchAsync(
            It.Is<SearchCriteria>(c =>
                c.Query == "test" &&
                c.Category == "Fiction" &&
                c.Status == BookStatus.Available &&
                c.MinYear == 2000 &&
                c.MaxYear == 2023 &&
                c.SortBy == "title" &&
                c.SortDescending == true &&
                c.Page == 2 &&
                c.PageSize == 10),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithResults_ReturnsMappedBookResponses()
    {
        // Arrange
        var query = new SearchBooksQuery { SearchTerm = "Design" };
        var book = Book.Create("Design Patterns", "GoF", "9780201633610", 1994, "Classic", "Software");

        var pagedBooks = new PagedResult<Book>(
            new List<Book> { book },
            totalCount: 1,
            page: 1,
            pageSize: 20);

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedBooks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Design Patterns");
        result.Items[0].Author.Should().Be("GoF");
        result.Items[0].Isbn.Should().Be("9780201633610");
        result.Items[0].PublishedYear.Should().Be(1994);
        result.Items[0].Category.Should().Be("Software");
        result.Items[0].Status.Should().Be("Available");
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_NoResults_ReturnsEmptyPagedResult()
    {
        // Arrange
        var query = new SearchBooksQuery { SearchTerm = "NonExistent" };

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<Book>.Empty(1, 20));

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_DefaultPagination_Uses20PerPage()
    {
        // Arrange
        var query = new SearchBooksQuery();

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<Book>.Empty(1, 20));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.SearchAsync(
            It.Is<SearchCriteria>(c => c.Page == 1 && c.PageSize == 20),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullSearchTerm_PassesNullQueryToCriteria()
    {
        // Arrange
        var query = new SearchBooksQuery { SearchTerm = null };

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(PagedResult<Book>.Empty(1, 20));

        // Act
        await _handler.Handle(query, CancellationToken.None);

        // Assert
        _bookRepositoryMock.Verify(r => r.SearchAsync(
            It.Is<SearchCriteria>(c => c.Query == null),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MultipleResults_PreservesPaginationMetadata()
    {
        // Arrange
        var query = new SearchBooksQuery { SearchTerm = "book", Page = 2, PageSize = 5 };
        var books = new List<Book>
        {
            Book.Create("Book A", "Author A", "9780000000001", 2020, "Desc A", "Fiction"),
            Book.Create("Book B", "Author B", "9780000000002", 2021, "Desc B", "Science"),
        };

        var pagedBooks = new PagedResult<Book>(books, totalCount: 12, page: 2, pageSize: 5);

        _bookRepositoryMock
            .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedBooks);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(12);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalPages.Should().Be(3);
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeTrue();
    }
}
