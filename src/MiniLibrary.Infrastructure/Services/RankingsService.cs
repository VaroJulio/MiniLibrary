using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Application.Rankings.DTOs;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// EF Core-backed implementation of <see cref="IRankingsService"/>.
/// Computes ranking aggregations directly from the database.
/// </summary>
public sealed class RankingsService : IRankingsService
{
    private readonly AppDbContext _context;

    public RankingsService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookRankingItem>> GetBookRankingsAsync(
        string? category,
        int? yearFrom,
        int? yearTo,
        bool? availableOnly,
        string sortBy,
        bool sortDescending,
        CancellationToken ct)
    {
        var query = _context.Books
            .Where(b => b.TotalRatings >= 3);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(b => b.Category == category);

        if (yearFrom.HasValue)
            query = query.Where(b => b.PublishedYear >= yearFrom.Value);

        if (yearTo.HasValue)
            query = query.Where(b => b.PublishedYear <= yearTo.Value);

        if (availableOnly == true)
            query = query.Where(b => b.Status == BookStatus.Available);

        // Get loan counts per book
        var booksWithLoans = query.Select(b => new
        {
            b.Id,
            b.Title,
            b.Author,
            b.Category,
            b.AverageRating,
            b.TotalRatings,
            b.Status,
            TotalLoans = b.Loans.Count
        });

        // Apply sorting
        booksWithLoans = sortBy.ToLowerInvariant() switch
        {
            "totalratings" => sortDescending
                ? booksWithLoans.OrderByDescending(b => b.TotalRatings)
                : booksWithLoans.OrderBy(b => b.TotalRatings),
            "totalloans" => sortDescending
                ? booksWithLoans.OrderByDescending(b => b.TotalLoans)
                : booksWithLoans.OrderBy(b => b.TotalLoans),
            _ => sortDescending
                ? booksWithLoans.OrderByDescending(b => b.AverageRating)
                : booksWithLoans.OrderBy(b => b.AverageRating)
        };

        var items = await booksWithLoans.ToListAsync(ct);

        return items.Select((b, index) => new BookRankingItem(
            index + 1,
            b.Id,
            b.Title,
            b.Author,
            b.Category,
            b.AverageRating,
            b.TotalRatings,
            b.TotalLoans,
            b.Status.ToString())).ToList();
    }

    public async Task<List<ReaderRankingItem>> GetReaderRankingsAsync(
        string period,
        CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var startDate = period.ToLowerInvariant() switch
        {
            "30d" => now.AddDays(-30),
            "90d" => now.AddDays(-90),
            "12m" => now.AddMonths(-12),
            _ => DateTime.MinValue // all-time
        };

        // Get readers with returned loans in period
        var readers = await _context.BookLoans
            .Where(l => l.ReturnedAt != null && l.ReturnedAt >= startDate)
            .Include(l => l.User)
            .Include(l => l.Book)
            .GroupBy(l => new { l.UserId, l.User.FullName })
            .Select(g => new
            {
                g.Key.UserId,
                Name = g.Key.FullName,
                BooksRead = g.Count(),
                MostReadCategory = g
                    .GroupBy(l => l.Book.Category)
                    .OrderByDescending(cg => cg.Count())
                    .Select(cg => cg.Key)
                    .FirstOrDefault() ?? "Unknown"
            })
            .OrderByDescending(r => r.BooksRead)
            .ToListAsync(ct);

        // Get average rating given by each reader
        var readerIds = readers.Select(r => r.UserId).ToList();
        var avgRatings = await _context.Ratings
            .Where(r => readerIds.Contains(r.UserId))
            .GroupBy(r => r.UserId)
            .Select(g => new { UserId = g.Key, Avg = g.Average(r => (decimal)r.Score) })
            .ToDictionaryAsync(x => x.UserId, x => x.Avg, ct);

        return readers.Select((r, index) => new ReaderRankingItem(
            index + 1,
            r.UserId,
            r.Name,
            r.BooksRead,
            r.MostReadCategory,
            avgRatings.GetValueOrDefault(r.UserId, 0m))).ToList();
    }

    public async Task<List<CategoryRankingItem>> GetCategoryRankingsAsync(CancellationToken ct)
    {
        var categories = await _context.Books
            .Where(b => b.TotalRatings >= 1)
            .GroupBy(b => b.Category)
            .Select(g => new
            {
                Category = g.Key,
                AverageRating = g.Average(b => b.AverageRating),
                TotalBooks = g.Count(),
                BestBook = g.OrderByDescending(b => b.AverageRating).First()
            })
            .OrderByDescending(c => c.AverageRating)
            .ToListAsync(ct);

        return categories.Select(c => new CategoryRankingItem(
            c.Category,
            Math.Round(c.AverageRating, 1, MidpointRounding.AwayFromZero),
            c.TotalBooks,
            c.BestBook.Title,
            c.BestBook.Author)).ToList();
    }
}
