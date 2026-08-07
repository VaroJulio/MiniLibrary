namespace MiniLibrary.Application.Dashboard.DTOs;

/// <summary>
/// DTO for dashboard overview statistics (Req 8.1-8.2).
/// </summary>
public sealed record DashboardStatsResponse(
    int TotalBooks,
    int AvailableBooks,
    int CheckedOutBooks,
    int ActiveLoans,
    Dictionary<string, int> UsersByRole);

/// <summary>
/// DTO for loan metrics (Req 8.3-8.4).
/// </summary>
public sealed record LoanMetricsResponse(
    int LoansLast7Days,
    int LoansLast30Days,
    int LoansLast12Months,
    List<CategoryMetric> PopularCategories,
    List<TopBorrowedBook> TopBorrowedBooks);

/// <summary>
/// Category with loan count.
/// </summary>
public sealed record CategoryMetric(string Category, int LoanCount);

/// <summary>
/// A top-borrowed book with its borrow count.
/// </summary>
public sealed record TopBorrowedBook(Guid BookId, string Title, string Author, int BorrowCount);
