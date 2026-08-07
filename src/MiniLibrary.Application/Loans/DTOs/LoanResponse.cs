namespace MiniLibrary.Application.Loans.DTOs;

/// <summary>
/// DTO representing a book loan in API responses.
/// </summary>
public record LoanResponse(
    Guid Id,
    Guid BookId,
    string BookTitle,
    DateTime BorrowedAt,
    DateTime DueDate,
    DateTime? ReturnedAt);
