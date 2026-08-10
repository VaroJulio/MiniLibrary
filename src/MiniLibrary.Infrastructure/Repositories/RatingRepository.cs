using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IRatingRepository"/>.
/// Ratings are ordered by CreatedAt descending for book rating queries.
/// </summary>
public sealed class RatingRepository : IRatingRepository
{
    private readonly AppDbContext _context;

    public RatingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Rating?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken ct)
    {
        return await _context.Ratings
            .FirstOrDefaultAsync(r => r.UserId == userId && r.BookId == bookId, ct);
    }

    public async Task<Rating?> GetByLoanIdAsync(Guid loanId, CancellationToken ct)
    {
        return await _context.Ratings
            .FirstOrDefaultAsync(r => r.LoanId == loanId, ct);
    }

    public async Task<Rating?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Ratings
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<PagedResult<Rating>> GetBookRatingsAsync(Guid bookId, PaginationParams paging, CancellationToken ct)
    {
        var baseQuery = _context.Ratings.Where(r => r.BookId == bookId);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Rating>(items, totalCount, paging.Page, paging.PageSize);
    }

    public async Task<List<Rating>> GetRecentBookRatingsAsync(Guid bookId, int count, CancellationToken ct)
    {
        return await _context.Ratings
            .Where(r => r.BookId == bookId)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync(ct);
    }

    public async Task<(decimal Average, int Count)> CalculateBookAverageAsync(Guid bookId, CancellationToken ct)
    {
        var ratings = _context.Ratings.Where(r => r.BookId == bookId);

        var count = await ratings.CountAsync(ct);
        if (count == 0)
            return (0m, 0);

        var average = await ratings.AverageAsync(r => (decimal)r.Score, ct);
        return (Math.Round(average, 1), count);
    }

    public async Task<int> GetUserRatingCountAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Ratings.CountAsync(r => r.UserId == userId, ct);
    }

    public async Task<int> GetUserUsefulReviewCountAsync(Guid userId, int minVotes, CancellationToken ct)
    {
        return await _context.Ratings
            .CountAsync(r => r.UserId == userId && r.UsefulVotes >= minVotes, ct);
    }

    public async Task<PagedResult<Rating>> GetUserRatingsAsync(Guid userId, PaginationParams paging, CancellationToken ct)
    {
        var baseQuery = _context.Ratings.Where(r => r.UserId == userId);

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(r => r.Book)
            .Include(r => r.User)
            .OrderByDescending(r => r.UpdatedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Rating>(items, totalCount, paging.Page, paging.PageSize);
    }

    public async Task<PagedResult<Rating>> GetRecentRatingsAsync(PaginationParams paging, CancellationToken ct)
    {
        var baseQuery = _context.Ratings.AsQueryable();

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .Include(r => r.User)
            .Include(r => r.Book)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Rating>(items, totalCount, paging.Page, paging.PageSize);
    }

    public async Task AddAsync(Rating rating, CancellationToken ct)
    {
        await _context.Ratings.AddAsync(rating, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Rating rating, CancellationToken ct)
    {
        _context.Ratings.Update(rating);
        await _context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(Rating rating, CancellationToken ct)
    {
        _context.Ratings.Remove(rating);
        await _context.SaveChangesAsync(ct);
    }
}
