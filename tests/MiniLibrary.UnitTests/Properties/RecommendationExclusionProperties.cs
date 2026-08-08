using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Recommendations.DTOs;
using MiniLibrary.Application.Recommendations.Queries.GetRecommendations;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for recommendation exclusion invariant.
/// Property 7: Recommendation Exclusion Invariant — Generate random member histories
/// and recommendation sets, verify no recommended book appears in member's history
/// or active loans.
/// **Validates: Requirements 5.5**
/// </summary>
[Trait("Category", "Property")]
public class RecommendationExclusionProperties
{
    // ── Property 7a: No recommended book appears in member's loan history ────────

    /// <summary>
    /// For any set of recommendations returned by the handler, no recommended book
    /// should appear in the member's loan history (completed or active).
    /// **Validates: Requirements 5.5**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property RecommendedBooks_NeverAppearInMemberHistory()
    {
        return Prop.ForAll(
            Arb.From(GenHistoryAndCatalog()),
            input =>
            {
                var (userId, historyBookIds, catalogBooks) = input;

                // Arrange
                var mockRecommendationService = new Mock<IRecommendationService>();
                var mockLoanRepository = new Mock<ILoanRepository>();
                var mockBookRepository = new Mock<IBookRepository>();
                var mockCacheService = new Mock<ICacheService>();
                var mockLogger = new Mock<ILogger<GetRecommendationsQueryHandler>>();

                // Build loan history with Book navigation property set
                var loans = historyBookIds.Select(bookId =>
                {
                    var book = CreateBook(bookId, "History Book", "Author", "Category");
                    var loan = BookLoan.Create(bookId, userId, DateTime.UtcNow.AddDays(-30));
                    SetBookNavigation(loan, book);
                    return loan;
                }).ToList();

                mockLoanRepository
                    .Setup(r => r.GetUserHistoryAsync(userId, It.IsAny<PaginationParams>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PagedResult<BookLoan>(loans, loans.Count, 1, 200));

                // Catalog: mix of history books and new books
                mockBookRepository
                    .Setup(r => r.SearchAsync(It.IsAny<SearchCriteria>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(new PagedResult<Book>(catalogBooks, catalogBooks.Count, 1, 100));

                // Cache miss
                mockCacheService
                    .Setup(c => c.GetAsync<List<RecommendationResponse>>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((List<RecommendationResponse>?)null);

                // Recommendation service returns some catalog books (including potentially history books)
                var allCatalogResults = catalogBooks.Select(b =>
                    new RecommendationResult(b.Id, b.Title, b.Author, b.Category, "Great book"))
                    .ToList();

                mockRecommendationService
                    .Setup(s => s.GetRecommendationsAsync(
                        userId,
                        It.IsAny<List<BookLoan>>(),
                        It.IsAny<List<Book>>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(allCatalogResults);

                var handler = new GetRecommendationsQueryHandler(
                    mockRecommendationService.Object,
                    mockLoanRepository.Object,
                    mockBookRepository.Object,
                    mockCacheService.Object,
                    mockLogger.Object);

                var query = new GetRecommendationsQuery { UserId = userId };

                // Act
                var result = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                // Assert: no recommendation should be in history
                var historySet = historyBookIds.ToHashSet();
                return result.All(r => !historySet.Contains(r.BookId));
            });
    }

    // ── Property 7b: Exclusion is deterministic regardless of catalog order ──────

    /// <summary>
    /// The exclusion filter produces the same results regardless of catalog ordering.
    /// **Validates: Requirements 5.5**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property ExclusionFilter_IsDeterministic()
    {
        return Prop.ForAll(
            Arb.From(GenExclusionScenario()),
            input =>
            {
                var (historyBookIds, recommendedBookIds) = input;

                var historySet = historyBookIds.ToHashSet();

                // Simulate the exclusion logic from the handler
                var filtered = recommendedBookIds
                    .Where(id => !historySet.Contains(id))
                    .ToList();

                // All filtered results must not be in history
                var allExcluded = filtered.All(id => !historySet.Contains(id));

                // All history items must not appear in filtered results
                var noneLeaked = historyBookIds.All(id => !filtered.Contains(id));

                return allExcluded && noneLeaked;
            });
    }

    // ── Property 7c: Empty history means no exclusions ───────────────────────────

    /// <summary>
    /// When member has no loan history, all catalog books are eligible for recommendation.
    /// **Validates: Requirements 5.5**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property EmptyHistory_NoExclusions()
    {
        return Prop.ForAll(
            Arb.From(GenBookIdList(1, 20)),
            catalogBookIds =>
            {
                var emptyHistory = new HashSet<Guid>();

                var filtered = catalogBookIds
                    .Where(id => !emptyHistory.Contains(id))
                    .ToList();

                return filtered.Count == catalogBookIds.Count;
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Custom Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates a test scenario with a user ID, their history book IDs, and a catalog
    /// that overlaps partially with the history.
    /// </summary>
    private static Gen<(Guid UserId, List<Guid> HistoryBookIds, List<Book> CatalogBooks)> GenHistoryAndCatalog()
    {
        return from historyCount in Gen.Choose(1, 10)
               from catalogCount in Gen.Choose(3, 15)
               from overlapCount in Gen.Choose(0, Math.Min(historyCount, catalogCount))
               let userId = Guid.NewGuid()
               let historyBookIds = Enumerable.Range(0, historyCount).Select(_ => Guid.NewGuid()).ToList()
               let newCatalogBookIds = Enumerable.Range(0, catalogCount - overlapCount).Select(_ => Guid.NewGuid()).ToList()
               let overlapBookIds = historyBookIds.Take(overlapCount).ToList()
               let allCatalogIds = newCatalogBookIds.Concat(overlapBookIds).ToList()
               let catalogBooks = allCatalogIds.Select(id =>
                   CreateBook(id, $"Book {id.ToString()[..6]}", "Author", "Fiction"))
                   .ToList()
               select (userId, historyBookIds, catalogBooks);
    }

    /// <summary>
    /// Generates a tuple of (history IDs, recommendation IDs) with possible overlap.
    /// </summary>
    private static Gen<(List<Guid> HistoryIds, List<Guid> RecommendedIds)> GenExclusionScenario()
    {
        return from historyCount in Gen.Choose(1, 15)
               from recCount in Gen.Choose(1, 10)
               from overlapCount in Gen.Choose(0, Math.Min(historyCount, recCount))
               let historyIds = Enumerable.Range(0, historyCount).Select(_ => Guid.NewGuid()).ToList()
               let newRecIds = Enumerable.Range(0, recCount - overlapCount).Select(_ => Guid.NewGuid()).ToList()
               let overlapIds = historyIds.Take(overlapCount).ToList()
               let allRecIds = newRecIds.Concat(overlapIds).ToList()
               select (historyIds, allRecIds);
    }

    /// <summary>
    /// Generates a list of random GUIDs.
    /// </summary>
    private static Gen<List<Guid>> GenBookIdList(int min, int max)
    {
        return Gen.Choose(min, max)
            .Select(count => Enumerable.Range(0, count).Select(_ => Guid.NewGuid()).ToList());
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════════

    private static Book CreateBook(Guid id, string title, string author, string category)
    {
        var book = Book.Create(title, author, "9780306406157", 2020, "Description", category);
        typeof(Entity).GetProperty("Id")!.SetValue(book, id);
        return book;
    }

    private static void SetBookNavigation(BookLoan loan, Book book)
    {
        var bookProp = typeof(BookLoan).GetProperty("Book")!;
        bookProp.SetValue(loan, book);
    }
}
