using MiniLibrary.Application.Dashboard.DTOs;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for computing dashboard statistics and loan metrics.
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Returns overview stats: total books, available, checked out, active loans, users by role.
    /// </summary>
    Task<DashboardStatsResponse> GetStatsAsync(CancellationToken ct);

    /// <summary>
    /// Returns loan metrics: loans by period (7d, 30d, 12m), popular categories, top 10 most-borrowed books.
    /// </summary>
    Task<LoanMetricsResponse> GetLoanMetricsAsync(CancellationToken ct);
}
