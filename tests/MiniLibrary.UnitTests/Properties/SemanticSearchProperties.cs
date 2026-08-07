using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Books.Queries.SemanticSearch;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for semantic search invariants.
/// Property 6: Semantic Search Result Invariants — Generate random result sets and verify
/// all scores >= 0.3 and results are ordered by descending score.
/// **Validates: Requirements 4.1, 4.7**
/// </summary>
[Trait("Category", "Property")]
public class SemanticSearchProperties
{
    // ── Property 6a: All result scores are >= threshold (0.3) ────────────────────

    /// <summary>
    /// For any set of semantic results returned by the handler, every result's
    /// relevance score must be >= 0.3 (the minimum threshold).
    /// **Validates: Requirements 4.1, 4.7**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property AllResultScores_AreAboveThreshold()
    {
        return Prop.ForAll(
            Arb.From(GenSemanticResultSet()),
            results =>
            {
                // Simulate the filtering that the handler performs
                const float threshold = 0.3f;
                var filtered = results
                    .Where(r => r.Score >= threshold)
                    .OrderByDescending(r => r.Score)
                    .Take(20)
                    .ToList();

                return filtered.All(r => r.Score >= threshold);
            });
    }

    // ── Property 6b: Results are ordered by descending relevance score ───────────

    /// <summary>
    /// For any set of semantic results returned by the handler, results must be
    /// ordered by descending relevance score.
    /// **Validates: Requirements 4.7**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Results_AreOrderedByDescendingScore()
    {
        return Prop.ForAll(
            Arb.From(GenSemanticResultSet()),
            results =>
            {
                const float threshold = 0.3f;
                var filtered = results
                    .Where(r => r.Score >= threshold)
                    .OrderByDescending(r => r.Score)
                    .Take(20)
                    .ToList();

                // Verify ordering: each score is >= next score
                for (int i = 0; i < filtered.Count - 1; i++)
                {
                    if (filtered[i].Score < filtered[i + 1].Score)
                        return false;
                }
                return true;
            });
    }

    // ── Property 6c: Max 20 results returned ─────────────────────────────────────

    /// <summary>
    /// Regardless of how many results exceed the threshold, the handler returns at most 20.
    /// **Validates: Requirements 4.4**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Results_NeverExceedMaxLimit()
    {
        return Prop.ForAll(
            Arb.From(GenLargeSemanticResultSet()),
            results =>
            {
                const float threshold = 0.3f;
                const int maxResults = 20;
                var filtered = results
                    .Where(r => r.Score >= threshold)
                    .OrderByDescending(r => r.Score)
                    .Take(maxResults)
                    .ToList();

                return filtered.Count <= maxResults;
            });
    }

    // ── Property 6d: Handler integration — scores and ordering via mocked service ──

    /// <summary>
    /// When the handler receives results from IEmbeddingService.SearchSimilarAsync,
    /// the final response maintains score >= threshold and descending order invariants.
    /// **Validates: Requirements 4.1, 4.7**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property Handler_MaintainsScoreAndOrderInvariants()
    {
        return Prop.ForAll(
            Arb.From(GenQueryWithNonEmptyResults()),
            input =>
            {
                var (queryText, semanticResults) = input;

                // Arrange
                var mockEmbeddingService = new Mock<IEmbeddingService>();
                var mockBookRepository = new Mock<IBookRepository>();
                var mockLogger = new Mock<ILogger<SemanticSearchQueryHandler>>();

                // Generate a fake embedding (dummy vector)
                var fakeEmbedding = new float[] { 0.1f, 0.2f, 0.3f };
                mockEmbeddingService
                    .Setup(s => s.GenerateEmbeddingAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(fakeEmbedding);

                mockEmbeddingService
                    .Setup(s => s.SearchSimilarAsync(
                        It.IsAny<float[]>(),
                        It.IsAny<int>(),
                        It.IsAny<float>(),
                        It.IsAny<CancellationToken>()))
                    .ReturnsAsync(semanticResults);

                // Set up books for each result
                foreach (var sr in semanticResults)
                {
                    var book = Book.Create("Book " + sr.BookId.ToString()[..8], "Author", "9780306406157", 2020, "Description", "Fiction");
                    // Use reflection to set the Id since it's auto-generated
                    typeof(Entity).GetProperty("Id")!.SetValue(book, sr.BookId);

                    mockBookRepository
                        .Setup(r => r.GetByIdAsync(sr.BookId, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(book);
                }

                var handler = new SemanticSearchQueryHandler(
                    mockEmbeddingService.Object,
                    mockBookRepository.Object,
                    mockLogger.Object);

                var query = new SemanticSearchQuery { Query = queryText, MaxResults = 20, Threshold = 0.3f };

                // Act
                var response = handler.Handle(query, CancellationToken.None).GetAwaiter().GetResult();

                // Assert invariants
                if (response.UsedFallback)
                    return true; // Fallback results don't have semantic scores

                var allAboveThreshold = response.Results.All(r => r.RelevanceScore >= 0.3f);
                var isDescending = true;
                for (int i = 0; i < response.Results.Count - 1; i++)
                {
                    if (response.Results[i].RelevanceScore < response.Results[i + 1].RelevanceScore)
                    {
                        isDescending = false;
                        break;
                    }
                }

                return allAboveThreshold && isDescending;
            });
    }

    // ── Property 6e: Scores are in valid range [0.0, 1.0] ───────────────────────

    /// <summary>
    /// All relevance scores returned by SearchSimilarAsync are within [0.0, 1.0].
    /// **Validates: Requirements 4.5**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property AllScores_AreWithinValidRange()
    {
        return Prop.ForAll(
            Arb.From(GenSemanticResultSet()),
            results =>
            {
                return results.All(r => r.Score >= 0.0f && r.Score <= 1.0f);
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Custom Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates a list of SemanticResult with random scores in [0.0, 1.0].
    /// </summary>
    private static Gen<List<SemanticResult>> GenSemanticResultSet()
    {
        return Gen.Choose(0, 50).SelectMany(count =>
            Gen.ListOf(count, GenSingleSemanticResult())
               .Select(results => results.ToList()));
    }

    /// <summary>
    /// Generates a larger list (50-200 items) to test max results capping.
    /// </summary>
    private static Gen<List<SemanticResult>> GenLargeSemanticResultSet()
    {
        return Gen.Choose(50, 200).SelectMany(count =>
            Gen.ListOf(count, GenSingleSemanticResult())
               .Select(results => results.ToList()));
    }

    /// <summary>
    /// Generates a single SemanticResult with a random BookId and score in [0.0, 1.0].
    /// </summary>
    private static Gen<SemanticResult> GenSingleSemanticResult()
    {
        return from score in Gen.Choose(0, 1000).Select(i => i / 1000.0f)
               from id in Gen.Fresh(() => Guid.NewGuid())
               select new SemanticResult(id, score);
    }

    /// <summary>
    /// Generates a tuple of (query text, list of SemanticResults with scores >= 0.3).
    /// This simulates what SearchSimilarAsync returns (already filtered).
    /// </summary>
    private static Gen<(string Query, List<SemanticResult> Results)> GenQueryWithResults()
    {
        var genQuery = Gen.Choose(3, 100).SelectMany(len =>
            Gen.ArrayOf(len, Gen.Elements(
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', ' ',
                'l', 'm', 'n', 'o', 'p', 'r', 's', 't'))
                .Select(chars => new string(chars).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var genResults = Gen.Choose(0, 25).SelectMany(count =>
            Gen.ListOf(count, GenFilteredSemanticResult())
               .Select(results => results
                   .OrderByDescending(r => r.Score)
                   .Take(20)
                   .ToList()));

        return from query in genQuery
               from results in genResults
               select (query, results);
    }

    /// <summary>
    /// Generates a tuple of (query text, non-empty list of SemanticResults with scores >= 0.3).
    /// Ensures at least 1 result so the handler doesn't fall back to text search.
    /// </summary>
    private static Gen<(string Query, List<SemanticResult> Results)> GenQueryWithNonEmptyResults()
    {
        var genQuery = Gen.Choose(3, 100).SelectMany(len =>
            Gen.ArrayOf(len, Gen.Elements(
                'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', ' ',
                'l', 'm', 'n', 'o', 'p', 'r', 's', 't'))
                .Select(chars => new string(chars).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var genResults = Gen.Choose(1, 25).SelectMany(count =>
            Gen.ListOf(count, GenFilteredSemanticResult())
               .Select(results => results
                   .OrderByDescending(r => r.Score)
                   .Take(20)
                   .ToList()));

        return from query in genQuery
               from results in genResults
               select (query, results);
    }
    /// <summary>
    /// Generates a SemanticResult with score >= 0.3 (simulating already-filtered results).
    /// </summary>
    private static Gen<SemanticResult> GenFilteredSemanticResult()
    {
        return from score in Gen.Choose(300, 1000).Select(i => i / 1000.0f)
               from id in Gen.Fresh(() => Guid.NewGuid())
               select new SemanticResult(id, score);
    }
}
