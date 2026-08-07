namespace MiniLibrary.Application.Loans.DTOs;

/// <summary>
/// DTO representing an overdue loan in API responses.
/// Includes member and book details for Librarian/Admin review.
/// </summary>
public record OverdueLoanResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    Guid UserId,
    string UserName,
    string UserEmail,
    DateTime BorrowedAt,
    DateTime DueDate,
    int DaysOverdue);
