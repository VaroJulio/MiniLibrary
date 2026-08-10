using MiniLibrary.Domain.Events;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity tracking the lifecycle of a book loan.
/// </summary>
public class BookLoan : Entity
{
    public Guid BookId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime BorrowedAt { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateTime? ReturnedAt { get; private set; }

    // Navigation properties
    public Book Book { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // Required by EF Core
    private BookLoan() { }

    public static BookLoan Create(Guid bookId, Guid userId, DateTime borrowedAt, int loanDurationDays = 14)
    {
        if (bookId == Guid.Empty)
            throw new ArgumentException("BookId is required.", nameof(bookId));
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));
        if (loanDurationDays <= 0)
            throw new ArgumentOutOfRangeException(nameof(loanDurationDays), "Loan duration must be a positive number of days.");

        return new BookLoan
        {
            BookId = bookId,
            UserId = userId,
            BorrowedAt = borrowedAt,
            DueDate = borrowedAt.AddDays(loanDurationDays),
            ReturnedAt = null
        };
    }

    public bool IsActive => ReturnedAt is null;

    /// <summary>
    /// Determines if the loan is overdue at a specific point in time.
    /// Use this method in application logic and tests for deterministic behavior.
    /// </summary>
    public bool IsOverdueAt(DateTime utcNow) => IsActive && utcNow > DueDate;

    /// <summary>
    /// Calculates the number of days until the loan is due, relative to a specific point in time.
    /// Returns 0 if the loan has been returned. Returns negative values if overdue.
    /// </summary>
    public int DaysUntilDueAt(DateTime utcNow) => IsActive ? (int)(DueDate - utcNow).TotalDays : 0;

    /// <summary>Convenience property using current UTC time. Prefer IsOverdueAt() in testable code.</summary>
    public bool IsOverdue => IsOverdueAt(DateTime.UtcNow);

    /// <summary>Convenience property using current UTC time. Prefer DaysUntilDueAt() in testable code.</summary>
    public int DaysUntilDue => DaysUntilDueAt(DateTime.UtcNow);

    public void Return(DateTime returnedAt)
    {
        if (!IsActive)
            throw new InvalidOperationException("This loan has already been returned.");

        ReturnedAt = returnedAt;
        RaiseDomainEvent(new BookReturnedEvent(BookId, UserId, Id));
    }
}
