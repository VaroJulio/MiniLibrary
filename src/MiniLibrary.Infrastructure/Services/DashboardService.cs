using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Dashboard.DTOs;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// EF Core-backed implementation of <see cref="IDashboardService"/>.
/// Computes aggregate statistics directly from the database.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly AppDbContext _context;

    public DashboardService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct)
    {
        var totalBooks = await _context.Books.CountAsync(ct);
        var availableBooks = await _context.Books
            .CountAsync(b => b.Status == BookStatus.Available, ct);
        var checkedOutBooks = await _context.Books
            .CountAsync(b => b.Status == BookStatus.CheckedOut, ct);
        var activeLoans = await _context.BookLoans
            .CountAsync(l => l.ReturnedAt == null, ct);

        var usersByRole = await _context.Users
            .Where(u => !u.IsDeleted)
            .GroupBy(u => u.Role)
            .Select(g => new { Role = g.Key.ToString(), Count = g.Count() })
            .ToDictionaryAsync(x => x.Role, x => x.Count, ct);

        return new DashboardStatsResponse(
            totalBooks,
            availableBooks,
            checkedOutBooks,
            activeLoans,
            usersByRole);
    }

    public async Task<LoanMetricsResponse> GetLoanMetricsAsync(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var sevenDaysAgo = now.AddDays(-7);
        var thirtyDaysAgo = now.AddDays(-30);
        var twelveMonthsAgo = now.AddMonths(-12);

        var loansLast7Days = await _context.BookLoans
            .CountAsync(l => l.BorrowedAt >= sevenDaysAgo, ct);
        var loansLast30Days = await _context.BookLoans
            .CountAsync(l => l.BorrowedAt >= thirtyDaysAgo, ct);
        var loansLast12Months = await _context.BookLoans
            .CountAsync(l => l.BorrowedAt >= twelveMonthsAgo, ct);

        // Popular categories (by loan count in last 12 months)
        // Use Join to avoid Include + GroupBy which EF Core cannot translate.
        var popularCategories = await _context.BookLoans
            .Where(l => l.BorrowedAt >= twelveMonthsAgo)
            .Join(
                _context.Books,
                loan => loan.BookId,
                book => book.Id,
                (loan, book) => new { book.Category })
            .GroupBy(x => x.Category)
            .Select(g => new CategoryMetric(g.Key, g.Count()))
            .OrderByDescending(c => c.LoanCount)
            .Take(10)
            .ToListAsync(ct);

        // Top 10 most-borrowed books (all time)
        // Use Join with simple key selectors for EF Core SQL translation.
        var topBooks = await _context.BookLoans
            .Join(
                _context.Books,
                loan => loan.BookId,
                book => book.Id,
                (loan, book) => new { book.Id, book.Title, book.Author })
            .GroupBy(x => new { x.Id, x.Title, x.Author })
            .Select(g => new TopBorrowedBook(g.Key.Id, g.Key.Title, g.Key.Author, g.Count()))
            .OrderByDescending(b => b.BorrowCount)
            .Take(10)
            .ToListAsync(ct);

        return new LoanMetricsResponse(
            loansLast7Days,
            loansLast30Days,
            loansLast12Months,
            popularCategories,
            topBooks);
    }
}
