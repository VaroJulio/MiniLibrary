using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of ILoanRepository.
/// </summary>
public sealed class LoanRepository : ILoanRepository
{
    private readonly AppDbContext _context;

    public LoanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> GetActiveLoanCountAsync(Guid userId, CancellationToken ct)
    {
        return await _context.BookLoans
            .CountAsync(l => l.UserId == userId && l.ReturnedAt == null, ct);
    }

    public async Task<BookLoan?> GetActiveLoanAsync(Guid bookId, Guid userId, CancellationToken ct)
    {
        return await _context.BookLoans
            .FirstOrDefaultAsync(l => l.BookId == bookId && l.UserId == userId && l.ReturnedAt == null, ct);
    }

    public async Task<BookLoan?> GetActiveLoanByBookAsync(Guid bookId, CancellationToken ct)
    {
        return await _context.BookLoans
            .FirstOrDefaultAsync(l => l.BookId == bookId && l.ReturnedAt == null, ct);
    }

    public async Task<PagedResult<BookLoan>> GetUserHistoryAsync(Guid userId, PaginationParams paging, CancellationToken ct)
    {
        var query = _context.BookLoans
            .Where(l => l.UserId == userId)
            .Include(l => l.Book)
            .OrderByDescending(l => l.BorrowedAt);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BookLoan>(items, totalCount, paging.Page, paging.PageSize);
    }

    public async Task<PagedResult<BookLoan>> GetOverdueLoansAsync(PaginationParams paging, CancellationToken ct)
    {
        var now = DateTime.UtcNow;

        var query = _context.BookLoans
            .Where(l => l.ReturnedAt == null && l.DueDate < now)
            .Include(l => l.Book)
            .Include(l => l.User)
            .OrderBy(l => l.DueDate);

        var totalCount = await query.CountAsync(ct);

        var items = await query
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<BookLoan>(items, totalCount, paging.Page, paging.PageSize);
    }

    public async Task<bool> HasCompletedLoanAsync(Guid bookId, Guid userId, CancellationToken ct)
    {
        return await _context.BookLoans
            .AnyAsync(l => l.BookId == bookId && l.UserId == userId && l.ReturnedAt != null, ct);
    }

    public async Task<BookLoan?> GetMostRecentCompletedLoanAsync(Guid bookId, Guid userId, CancellationToken ct)
    {
        return await _context.BookLoans
            .Where(l => l.BookId == bookId && l.UserId == userId && l.ReturnedAt != null)
            .OrderByDescending(l => l.ReturnedAt)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AddAsync(BookLoan loan, CancellationToken ct)
    {
        await _context.BookLoans.AddAsync(loan, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BookLoan loan, CancellationToken ct)
    {
        _context.BookLoans.Update(loan);
        await _context.SaveChangesAsync(ct);
    }
}
