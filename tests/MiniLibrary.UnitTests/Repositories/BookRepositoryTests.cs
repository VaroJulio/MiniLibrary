using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Infrastructure.Data;
using MiniLibrary.Infrastructure.Repositories;

namespace MiniLibrary.UnitTests.Repositories;

public class BookRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly BookRepository _repository;

    public BookRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new BookRepository(_context);
    }

    /// <summary>Flush pending EF changes (replaces the SaveChanges that was removed from repos).</summary>
    private Task SaveAsync() => _context.SaveChangesAsync();

    /// <summary>Helper: add book and persist to in-memory store.</summary>
    private async Task SeedBookAsync(Book book)
    {
        await _repository.AddAsync(book, CancellationToken.None);
        await SaveAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    private static Book CreateBook(
        string title = "Test Book",
        string author = "Test Author",
        string isbn = "9780306406157",
        int publishedYear = 2020,
        string description = "A test book",
        string category = "Fiction")
    {
        return Book.Create(title, author, isbn, publishedYear, description, category);
    }

    // ── GetByIdAsync ────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIdAsync_ExistingBook_ReturnsBook()
    {
        var book = CreateBook();
        await _repository.AddAsync(book, CancellationToken.None);
        await SaveAsync();

        var result = await _repository.GetByIdAsync(book.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(book.Id);
        result.Title.Should().Be("Test Book");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingBook_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid(), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_DeletedBook_ReturnsNull()
    {
        var book = CreateBook();
        await _repository.AddAsync(book, CancellationToken.None);
        await _repository.DeleteAsync(book, CancellationToken.None);
        await SaveAsync();

        var result = await _repository.GetByIdAsync(book.Id, CancellationToken.None);

        result.Should().BeNull();
    }

    // ── GetByIsbnAsync ──────────────────────────────────────────────────────────

    [Fact]
    public async Task GetByIsbnAsync_ExistingIsbn_ReturnsBook()
    {
        var book = CreateBook(isbn: "9780140449136");
        await _repository.AddAsync(book, CancellationToken.None);
        await SaveAsync();

        var result = await _repository.GetByIsbnAsync("9780140449136", CancellationToken.None);

        result.Should().NotBeNull();
        result!.ISBN.Should().Be("9780140449136");
    }

    [Fact]
    public async Task GetByIsbnAsync_NonExistingIsbn_ReturnsNull()
    {
        var result = await _repository.GetByIsbnAsync("9780306406157", CancellationToken.None);

        result.Should().BeNull();
    }

    // ── AddAsync ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_ValidBook_PersistsToDatabase()
    {
        var book = CreateBook();

        await _repository.AddAsync(book, CancellationToken.None);
        await SaveAsync();

        var stored = await _context.Books.FindAsync(book.Id);
        stored.Should().NotBeNull();
        stored!.Title.Should().Be("Test Book");
    }

    // ── UpdateAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_ExistingBook_PersistsChanges()
    {
        var book = CreateBook();
        await _repository.AddAsync(book, CancellationToken.None);
        await SaveAsync();

        book.Update("Updated Title", "Updated Author", "9780140449136", 2021, "Updated desc", "Science");
        await _repository.UpdateAsync(book, CancellationToken.None);
        await SaveAsync();

        var stored = await _context.Books.FindAsync(book.Id);
        stored!.Title.Should().Be("Updated Title");
        stored.Author.Should().Be("Updated Author");
        stored.Category.Should().Be("Science");
    }

    // ── DeleteAsync ─────────────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_ExistingBook_MarksAsDeleted()
    {
        var book = CreateBook();
        await _repository.AddAsync(book, CancellationToken.None);
        await SaveAsync();

        await _repository.DeleteAsync(book, CancellationToken.None);
        await SaveAsync();

        // Bypass global filter to check IsDeleted flag
        var stored = await _context.Books.IgnoreQueryFilters().FirstOrDefaultAsync(b => b.Id == book.Id);
        stored.Should().NotBeNull();
        stored!.IsDeleted.Should().BeTrue();
    }

    // ── SearchAsync — Text Search ───────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_ByTitle_ReturnsMatchingBooks()
    {
        await _repository.AddAsync(CreateBook(title: "Domain-Driven Design", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Clean Architecture", isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Query = "domain" };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("Domain-Driven Design");
    }

    [Fact]
    public async Task SearchAsync_ByAuthor_ReturnsMatchingBooks()
    {
        await _repository.AddAsync(CreateBook(author: "Robert Martin", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(author: "Eric Evans", isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Query = "martin" };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Author.Should().Be("Robert Martin");
    }

    [Fact]
    public async Task SearchAsync_ByIsbn_ReturnsMatchingBooks()
    {
        await _repository.AddAsync(CreateBook(isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Query = "9780306" };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].ISBN.Should().Be("9780306406157");
    }

    [Fact]
    public async Task SearchAsync_ByCategory_ReturnsMatchingBooks()
    {
        await _repository.AddAsync(CreateBook(category: "Science Fiction", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(category: "History", isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Query = "fiction" };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Category.Should().Be("Science Fiction");
    }

    [Fact]
    public async Task SearchAsync_CaseInsensitive_ReturnsMatches()
    {
        await _repository.AddAsync(CreateBook(title: "DOMAIN-DRIVEN DESIGN", isbn: "9780306406157"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Query = "domain-driven" };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task SearchAsync_NoQuery_ReturnsAllBooks()
    {
        await _repository.AddAsync(CreateBook(isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Query = null };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(2);
    }

    // ── SearchAsync — Filters ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_FilterByCategory_ReturnsOnlyMatchingCategory()
    {
        await _repository.AddAsync(CreateBook(category: "Fiction", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(category: "Science", isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Category = "Fiction" };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Category.Should().Be("Fiction");
    }

    [Fact]
    public async Task SearchAsync_FilterByStatus_ReturnsOnlyMatchingStatus()
    {
        var availableBook = CreateBook(isbn: "9780306406157");
        var checkedOutBook = CreateBook(isbn: "9780140449136");
        checkedOutBook.CheckOut();

        await _repository.AddAsync(availableBook, CancellationToken.None);
        await _repository.AddAsync(checkedOutBook, CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Status = BookStatus.Available };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Status.Should().Be(BookStatus.Available);
    }

    [Fact]
    public async Task SearchAsync_FilterByYearRange_ReturnsOnlyMatchingYears()
    {
        await _repository.AddAsync(CreateBook(publishedYear: 2000, isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(publishedYear: 2015, isbn: "9780140449136"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(publishedYear: 2023, isbn: "9780743273565"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { MinYear = 2010, MaxYear = 2020 };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].PublishedYear.Should().Be(2015);
    }

    [Fact]
    public async Task SearchAsync_CombinedQueryAndFilters_ReturnsIntersection()
    {
        await _repository.AddAsync(CreateBook(title: "C# in Depth", category: "Programming", publishedYear: 2019, isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "C# Cookbook", category: "Programming", publishedYear: 2010, isbn: "9780140449136"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "History of C#", category: "History", publishedYear: 2019, isbn: "9780743273565"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria
        {
            Query = "C#",
            Category = "Programming",
            MinYear = 2015
        };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.Items[0].Title.Should().Be("C# in Depth");
    }

    // ── SearchAsync — Pagination ────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_Pagination_ReturnsCorrectPage()
    {
        for (int i = 1; i <= 5; i++)
        {
            await _repository.AddAsync(
                CreateBook(title: $"Book {i}", isbn: $"978030640615{i}"),
                CancellationToken.None);
        }
        await SaveAsync();

        var criteria = new SearchCriteria { Page = 2, PageSize = 2 };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(5);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(2);
        result.TotalPages.Should().Be(3);
        result.HasNext.Should().BeTrue();
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_FirstPage_HasPreviousFalse()
    {
        for (int i = 1; i <= 3; i++)
        {
            await _repository.AddAsync(
                CreateBook(title: $"Book {i}", isbn: $"978030640615{i}"),
                CancellationToken.None);
        }
        await SaveAsync();

        var criteria = new SearchCriteria { Page = 1, PageSize = 2 };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.HasPrevious.Should().BeFalse();
        result.HasNext.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_LastPage_HasNextFalse()
    {
        for (int i = 1; i <= 3; i++)
        {
            await _repository.AddAsync(
                CreateBook(title: $"Book {i}", isbn: $"978030640615{i}"),
                CancellationToken.None);
        }
        await SaveAsync();

        var criteria = new SearchCriteria { Page = 2, PageSize = 2 };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.HasNext.Should().BeFalse();
        result.HasPrevious.Should().BeTrue();
    }

    [Fact]
    public async Task SearchAsync_OutOfRangePage_ReturnsEmptyWithMetadata()
    {
        await _repository.AddAsync(CreateBook(isbn: "9780306406157"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { Page = 10, PageSize = 20 };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(1);
        result.Page.Should().Be(10);
    }

    // ── SearchAsync — Sorting ───────────────────────────────────────────────────

    [Fact]
    public async Task SearchAsync_SortByTitleAscending_ReturnsInOrder()
    {
        await _repository.AddAsync(CreateBook(title: "Zebra", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Apple", isbn: "9780140449136"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Mango", isbn: "9780743273565"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { SortBy = "title", SortDescending = false };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Select(b => b.Title).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchAsync_SortByTitleDescending_ReturnsInOrder()
    {
        await _repository.AddAsync(CreateBook(title: "Zebra", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Apple", isbn: "9780140449136"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Mango", isbn: "9780743273565"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { SortBy = "title", SortDescending = true };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Select(b => b.Title).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task SearchAsync_SortByPublishedYear_ReturnsInOrder()
    {
        await _repository.AddAsync(CreateBook(title: "Old", publishedYear: 1990, isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "New", publishedYear: 2023, isbn: "9780140449136"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Mid", publishedYear: 2010, isbn: "9780743273565"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { SortBy = "publishedYear", SortDescending = true };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Select(b => b.PublishedYear).Should().BeInDescendingOrder();
    }

    [Fact]
    public async Task SearchAsync_SortByAuthor_ReturnsInOrder()
    {
        await _repository.AddAsync(CreateBook(author: "Zara", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(author: "Alice", isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { SortBy = "author", SortDescending = false };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Select(b => b.Author).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SearchAsync_DefaultSort_SortsByTitleAscending()
    {
        await _repository.AddAsync(CreateBook(title: "Zebra", isbn: "9780306406157"), CancellationToken.None);
        await _repository.AddAsync(CreateBook(title: "Apple", isbn: "9780140449136"), CancellationToken.None);
        await SaveAsync();

        var criteria = new SearchCriteria { SortBy = null };
        var result = await _repository.SearchAsync(criteria, CancellationToken.None);

        result.Items.Select(b => b.Title).Should().BeInAscendingOrder();
    }
}
