using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IBookRepository"/>.
/// Soft-delete filtering is handled by the global query filter on the Book entity.
/// </summary>
public class BookRepository : IBookRepository
{
    private readonly AppDbContext _context;

    public BookRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct)
    {
        return await _context.Books.AsNoTracking().FirstOrDefaultAsync(b => b.ISBN == isbn, ct);
    }

    public async Task<PagedResult<Book>> SearchAsync(SearchCriteria criteria, CancellationToken ct)
    {
        var query = _context.Books.AsNoTracking().AsQueryable();

        // Text search across title, author, ISBN, category.
        // SQL Server default collation (Latin1_General_CI_AS) is case-insensitive,
        // so no ToLower() is needed. Removing it allows index usage (SARGable).
        if (!string.IsNullOrWhiteSpace(criteria.Query))
        {
            var searchTerm = criteria.Query.Trim();
            query = query.Where(b =>
                EF.Functions.Like(b.Title, $"%{searchTerm}%") ||
                EF.Functions.Like(b.Author, $"%{searchTerm}%") ||
                EF.Functions.Like(b.ISBN, $"%{searchTerm}%") ||
                EF.Functions.Like(b.Category, $"%{searchTerm}%"));
        }

        // Filter by category
        if (!string.IsNullOrWhiteSpace(criteria.Category))
        {
            query = query.Where(b => b.Category == criteria.Category);
        }

        // Filter by status
        if (criteria.Status.HasValue)
        {
            query = query.Where(b => b.Status == criteria.Status.Value);
        }

        // Filter by year range
        if (criteria.MinYear.HasValue)
        {
            query = query.Where(b => b.PublishedYear >= criteria.MinYear.Value);
        }

        if (criteria.MaxYear.HasValue)
        {
            query = query.Where(b => b.PublishedYear <= criteria.MaxYear.Value);
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync(ct);

        // Apply sorting
        query = ApplySorting(query, criteria.SortBy, criteria.SortDescending);

        // Apply pagination
        var page = criteria.Page < 1 ? 1 : criteria.Page;
        var pageSize = criteria.PageSize < 1 ? 20 : Math.Min(criteria.PageSize, 100);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Book>(items, totalCount, page, pageSize);
    }

    public async Task AddAsync(Book book, CancellationToken ct)
    {
        await _context.Books.AddAsync(book, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Book book, CancellationToken ct)
    {
        _context.Books.Update(book);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Book book, CancellationToken ct)
    {
        book.Delete();
        await _context.SaveChangesAsync(ct);
    }

    private static IQueryable<Book> ApplySorting(IQueryable<Book> query, string? sortBy, bool descending)
    {
        return sortBy?.ToLowerInvariant() switch
        {
            "title" => descending
                ? query.OrderByDescending(b => b.Title)
                : query.OrderBy(b => b.Title),
            "author" => descending
                ? query.OrderByDescending(b => b.Author)
                : query.OrderBy(b => b.Author),
            "publishedyear" => descending
                ? query.OrderByDescending(b => b.PublishedYear)
                : query.OrderBy(b => b.PublishedYear),
            "averagerating" => descending
                ? query.OrderByDescending(b => b.AverageRating)
                : query.OrderBy(b => b.AverageRating),
            "totalratings" => descending
                ? query.OrderByDescending(b => b.TotalRatings)
                : query.OrderBy(b => b.TotalRatings),
            _ => query.OrderBy(b => b.Title) // Default sort by title ascending
        };
    }
}
