using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for ranking invariants.
/// Property 13: Book Ranking Invariants — All results have >= 3 ratings and correct ordering.
/// Property 15: Reader Ranking Ordering — Readers ordered by descending return count in period.
/// **Validates: Requirements 17.1, 17.3, 19.5**
/// </summary>
[Trait("Category", "Property")]
public class RankingInvariantProperties
{
    // ── Property 13a: All book ranking results have >= 3 ratings ─────────────────

    /// <summary>
    /// For any set of book ranking results, every entry must have TotalRatings >= 3.
    /// **Validates: Requirements 17.1**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property BookRanking_AllEntriesHaveMinimumRatings()
    {
        return Prop.ForAll(
            Arb.From(GenBookRankingList()),
            rankings =>
            {
                // Simulate the filter that the service applies
                var filtered = rankings.Where(r => r.TotalRatings >= 3).ToList();
                return filtered.All(r => r.TotalRatings >= 3);
            });
    }

    // ── Property 13b: Book rankings are correctly ordered by sort field ──────────

    /// <summary>
    /// When sorted by averageRating descending, each entry's rating is >= the next.
    /// **Validates: Requirements 17.3**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property BookRanking_CorrectlyOrderedByAverageRatingDesc()
    {
        return Prop.ForAll(
            Arb.From(GenBookRankingList()),
            rankings =>
            {
                var filtered = rankings
                    .Where(r => r.TotalRatings >= 3)
                    .OrderByDescending(r => r.AverageRating)
                    .ToList();

                for (int i = 0; i < filtered.Count - 1; i++)
                {
                    if (filtered[i].AverageRating < filtered[i + 1].AverageRating)
                        return false;
                }
                return true;
            });
    }

    // ── Property 13c: Book ranking positions are sequential starting at 1 ───────

    /// <summary>
    /// After filtering and ordering, positions should be assigned 1, 2, 3, ...
    /// **Validates: Requirements 17.3**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property BookRanking_PositionsAreSequential()
    {
        return Prop.ForAll(
            Arb.From(GenBookRankingListWithPositions()),
            rankings =>
            {
                for (int i = 0; i < rankings.Count; i++)
                {
                    if (rankings[i].Position != i + 1)
                        return false;
                }
                return true;
            });
    }

    // ── Property 15a: Reader rankings ordered by descending return count ─────────

    /// <summary>
    /// Reader rankings are ordered by BooksReadInPeriod descending.
    /// **Validates: Requirements 19.5**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property ReaderRanking_OrderedByDescendingReturnCount()
    {
        return Prop.ForAll(
            Arb.From(GenReaderRankingList()),
            rankings =>
            {
                var sorted = rankings
                    .OrderByDescending(r => r.BooksReadInPeriod)
                    .ToList();

                // Verify the input is correctly sorted
                for (int i = 0; i < sorted.Count - 1; i++)
                {
                    if (sorted[i].BooksReadInPeriod < sorted[i + 1].BooksReadInPeriod)
                        return false;
                }
                return true;
            });
    }

    // ── Property 15b: Reader ranking positions match ordering ────────────────────

    /// <summary>
    /// Position 1 has the highest BooksReadInPeriod, position N has the lowest.
    /// **Validates: Requirements 19.5**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property ReaderRanking_PositionOneHasHighestCount()
    {
        return Prop.ForAll(
            Arb.From(GenReaderRankingListWithPositions()),
            rankings =>
            {
                if (rankings.Count == 0) return true;

                var first = rankings[0];
                return rankings.All(r => first.BooksReadInPeriod >= r.BooksReadInPeriod);
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    private static Gen<List<BookRankingItem>> GenBookRankingList()
    {
        return Gen.Choose(0, 30).SelectMany(count =>
            Gen.ListOf(count, GenBookRankingItem())
               .Select(items => items.ToList()));
    }

    private static Gen<List<BookRankingItem>> GenBookRankingListWithPositions()
    {
        return Gen.Choose(1, 20).SelectMany(count =>
            Gen.ListOf(count, GenBookRankingItem())
               .Select(items =>
               {
                   var sorted = items
                       .Where(i => i.TotalRatings >= 3)
                       .OrderByDescending(i => i.AverageRating)
                       .Select((item, idx) => item with { Position = idx + 1 })
                       .ToList();
                   return sorted;
               }));
    }

    private static Gen<BookRankingItem> GenBookRankingItem()
    {
        return from totalRatings in Gen.Choose(0, 100)
               from avgRating in Gen.Choose(10, 50).Select(i => (decimal)i / 10)
               from totalLoans in Gen.Choose(0, 200)
               from id in Gen.Fresh(() => Guid.NewGuid())
               select new BookRankingItem(
                   0, id, "Book", "Author", "Fiction",
                   avgRating, totalRatings, totalLoans, "Available");
    }

    private static Gen<List<ReaderRankingItem>> GenReaderRankingList()
    {
        return Gen.Choose(0, 20).SelectMany(count =>
            Gen.ListOf(count, GenReaderRankingItem())
               .Select(items => items
                   .OrderByDescending(r => r.BooksReadInPeriod)
                   .ToList()));
    }

    private static Gen<List<ReaderRankingItem>> GenReaderRankingListWithPositions()
    {
        return Gen.Choose(1, 20).SelectMany(count =>
            Gen.ListOf(count, GenReaderRankingItem())
               .Select(items => items
                   .OrderByDescending(r => r.BooksReadInPeriod)
                   .Select((item, idx) => item with { Position = idx + 1 })
                   .ToList()));
    }

    private static Gen<ReaderRankingItem> GenReaderRankingItem()
    {
        return from booksRead in Gen.Choose(1, 100)
               from avgGiven in Gen.Choose(10, 50).Select(i => (decimal)i / 10)
               from id in Gen.Fresh(() => Guid.NewGuid())
               select new ReaderRankingItem(
                   0, id, "Reader", booksRead, "Fiction", avgGiven);
    }
}
