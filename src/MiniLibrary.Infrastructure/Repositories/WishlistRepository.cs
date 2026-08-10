using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IWishlistRepository"/>.
/// Wishlist entries include Book navigation for display purposes.
/// </summary>
public sealed class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;

    public WishlistRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<WishlistEntry>> GetUserWishlistAsync(Guid userId, PaginationParams paging, CancellationToken ct)
    {
        var baseQuery = _context.WishlistEntries.AsNoTracking().Where(w => w.UserId == userId);

        var totalCount = await baseQuery.CountAsync(ct);

        var pageSize = Math.Min(paging.PageSize, 20);

        var items = await baseQuery
            .Include(w => w.Book)
            .OrderByDescending(w => w.AddedAt)
            .Skip((paging.Page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<WishlistEntry>(items, totalCount, paging.Page, pageSize);
    }

    public async Task<WishlistEntry?> GetEntryAsync(Guid userId, Guid bookId, CancellationToken ct)
    {
        return await _context.WishlistEntries
            .FirstOrDefaultAsync(w => w.UserId == userId && w.BookId == bookId, ct);
    }

    public async Task<int> GetUserWishlistCountAsync(Guid userId, CancellationToken ct)
    {
        return await _context.WishlistEntries.AsNoTracking()
            .CountAsync(w => w.UserId == userId, ct);
    }

    public async Task<List<WishlistEntry>> GetBookWatchersAsync(Guid bookId, CancellationToken ct)
    {
        return await _context.WishlistEntries.AsNoTracking()
            .Where(w => w.BookId == bookId)
            .Include(w => w.User)
            .ToListAsync(ct);
    }

    public async Task AddAsync(WishlistEntry entry, CancellationToken ct)
    {
        await _context.WishlistEntries.AddAsync(entry, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(WishlistEntry entry, CancellationToken ct)
    {
        _context.WishlistEntries.Remove(entry);
        await _context.SaveChangesAsync(ct);
    }
}
