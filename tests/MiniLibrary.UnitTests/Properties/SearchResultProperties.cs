using AutoMapper;
using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Mappings;
using MiniLibrary.Application.Books.Queries.SearchBooks;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using Moq;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for search result correctness.
/// **Validates: Requirements 3.1, 3.3**
/// </summary>
[Trait("Category", "Property")]
public class SearchResultProperties
{
    private readonly IMapper _mapper;

    public SearchResultProperties()
    {
        var config = new MapperConfiguration(cfg => cfg.AddProfile<BookMappingProfile>());
        _mapper = config.CreateMapper();
    }

    /// <summary>
    /// Property 5: All filter parameters from SearchBooksQuery are correctly forwarded
    /// to the repository's SearchCriteria, ensuring category filter is passed.
    /// **Validates: Requirements 3.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SearchResults_AllMatchCategoryFilter_WhenCategorySpecified()
    {
        return Prop.ForAll(
            Arb.From(GenCategory()),
            category =>
            {
                // Arrange: create books matching category
                var matchingBooks = Enumerable.Range(0, 3)
                    .Select(i => Book.Create($"Book {i}", $"Author {i}", GenerateValidIsbn(i), 2020, "Desc", category))
                    .ToList();

                var mockRepo = new Mock<IBookRepository>();
                mockRepo.Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PagedResult<Book>(matchingBooks, matchingBooks.Count, 1, 20));

                var handler = new SearchBooksQueryHandler(mockRepo.Object, _mapper);
                var query = new SearchBooksQuery { Category = category, Page = 1, PageSize = 20 };

                // Act
                handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                // Assert: handler passes category to repository correctly
                mockRepo.Verify(r => r.SearchAsync(
                    It.Is<SearchCriteria>(c => c.Category == category),
                    It.IsAny<CancellationToken>()), Times.Once);

                return true;
            });
    }

    /// <summary>
    /// Property 5: Search results preserve pagination metadata correctly.
    /// TotalPages = ceil(TotalCount / PageSize), HasNext/HasPrevious are consistent.
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SearchResults_PaginationMetadata_IsConsistent()
    {
        return Prop.ForAll(
            Arb.From(GenPaginationParams()),
            args =>
            {
                var (totalCount, page, pageSize) = args;

                var books = Enumerable.Range(0, Math.Min(totalCount, pageSize))
                    .Select(i => Book.Create($"Book {i}", $"Author {i}", GenerateValidIsbn(i), 2020, "Desc", "Fiction"))
                    .ToList();

                var mockRepo = new Mock<IBookRepository>();
                mockRepo.Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PagedResult<Book>(books, totalCount, page, pageSize));

                var handler = new SearchBooksQueryHandler(mockRepo.Object, _mapper);
                var query = new SearchBooksQuery { Page = page, PageSize = pageSize };

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                var expectedTotalPages = (int)Math.Ceiling((double)totalCount / pageSize);

                return result.TotalCount == totalCount
                    && result.Page == page
                    && result.PageSize == pageSize
                    && result.TotalPages == expectedTotalPages
                    && result.HasNext == (page < expectedTotalPages)
                    && result.HasPrevious == (page > 1);
            });
    }

    /// <summary>
    /// Property 5: Empty search results return valid empty page with correct metadata.
    /// **Validates: Requirements 3.1**
    /// </summary>
    [Property(MaxTest = 50)]
    public Property SearchResults_EmptyResults_ReturnValidMetadata()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(1, 100)),
            pageSize =>
            {
                var mockRepo = new Mock<IBookRepository>();
                mockRepo.Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(PagedResult<Book>.Empty(1, pageSize));

                var handler = new SearchBooksQueryHandler(mockRepo.Object, _mapper);
                var query = new SearchBooksQuery { SearchTerm = "nonexistent", Page = 1, PageSize = pageSize };

                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                return result.Items.Count == 0
                    && result.TotalCount == 0
                    && result.Page == 1
                    && result.PageSize == pageSize
                    && result.TotalPages == 0
                    && !result.HasNext
                    && !result.HasPrevious;
            });
    }

    /// <summary>
    /// Property 5: SearchBooksQueryHandler passes all filter parameters correctly to the repository.
    /// **Validates: Requirements 3.1, 3.3**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property SearchResults_AllFiltersPassedToRepository()
    {
        return Prop.ForAll(
            Arb.From(GenSearchQuery()),
            queryParams =>
            {
                var (searchTerm, category, yearFrom, yearTo) = queryParams;

                var mockRepo = new Mock<IBookRepository>();
                mockRepo.Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(PagedResult<Book>.Empty(1, 20));

                var handler = new SearchBooksQueryHandler(mockRepo.Object, _mapper);
                var query = new SearchBooksQuery
                {
                    SearchTerm = searchTerm,
                    Category = category,
                    YearFrom = yearFrom,
                    YearTo = yearTo,
                    Page = 1,
                    PageSize = 20
                };

                handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                mockRepo.Verify(r => r.SearchAsync(
                    It.Is<SearchCriteria>(c =>
                        c.Query == searchTerm &&
                        c.Category == category &&
                        c.MinYear == yearFrom &&
                        c.MaxYear == yearTo),
                    It.IsAny<CancellationToken>()), Times.Once);

                return true;
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    private static Gen<string> GenCategory()
    {
        return Gen.Elements("Fiction", "Science", "History", "Technology", "Biography", "Fantasy", "Mystery");
    }

    private static Gen<(int totalCount, int page, int pageSize)> GenPaginationParams()
    {
        return from totalCount in Gen.Choose(0, 200)
               from pageSize in Gen.Choose(1, 50)
               from page in Gen.Choose(1, Math.Max(1, (int)Math.Ceiling((double)totalCount / pageSize)))
               select (totalCount, page, pageSize);
    }

    private static Gen<(string? searchTerm, string? category, int? yearFrom, int? yearTo)> GenSearchQuery()
    {
        return from useTerm in Gen.Elements(true, false)
               from term in Gen.Elements("code", "design", "history", "science")
               from useCat in Gen.Elements(true, false)
               from cat in GenCategory()
               from useYear in Gen.Elements(true, false)
               from yearFrom in Gen.Choose(1900, 2020)
               from yearTo in Gen.Choose(2000, 2025)
               select (
                   useTerm ? term : (string?)null,
                   useCat ? cat : (string?)null,
                   useYear ? yearFrom : (int?)null,
                   useYear ? yearTo : (int?)null
               );
    }

    private static string GenerateValidIsbn(int seed)
    {
        var prefix = "978000000";
        var partial = prefix + seed.ToString("D3");
        // Calculate ISBN-13 check digit
        var sum = 0;
        for (var i = 0; i < 12; i++)
        {
            var digit = partial[i] - '0';
            var weight = i % 2 == 0 ? 1 : 3;
            sum += digit * weight;
        }
        var checkDigit = (10 - (sum % 10)) % 10;
        return partial + checkDigit.ToString();
    }
}
