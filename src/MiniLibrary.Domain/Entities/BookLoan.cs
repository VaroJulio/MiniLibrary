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

    public bool IsOverdue => IsActive && DateTime.UtcNow > DueDate;

    public int DaysUntilDue => IsActive ? (int)(DueDate - DateTime.UtcNow).TotalDays : 0;

    public void Return(DateTime returnedAt)
    {
        if (!IsActive)
            throw new InvalidOperationException("This loan has already been returned.");

        ReturnedAt = returnedAt;
        RaiseDomainEvent(new BookReturnedEvent(BookId, UserId, Id));
    }
}
