using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for BookLoan entities.
/// </summary>
public interface ILoanRepository
{
    Task<int> GetActiveLoanCountAsync(Guid userId, CancellationToken ct);
    Task<BookLoan?> GetActiveLoanAsync(Guid bookId, Guid userId, CancellationToken ct);
    Task<BookLoan?> GetActiveLoanByBookAsync(Guid bookId, CancellationToken ct);
    Task<PagedResult<BookLoan>> GetUserHistoryAsync(Guid userId, PaginationParams paging, CancellationToken ct);
    Task<PagedResult<BookLoan>> GetOverdueLoansAsync(PaginationParams paging, CancellationToken ct);
    Task<bool> HasCompletedLoanAsync(Guid bookId, Guid userId, CancellationToken ct);
    Task AddAsync(BookLoan loan, CancellationToken ct);
    Task UpdateAsync(BookLoan loan, CancellationToken ct);
}
