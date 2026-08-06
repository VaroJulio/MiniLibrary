namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a book is returned (check-in).
/// Used to trigger badge evaluation and wishlist availability alerts.
/// </summary>
public sealed class BookReturnedEvent : DomainEvent
{
    public Guid BookId { get; }
    public Guid UserId { get; }
    public Guid LoanId { get; }

    public BookReturnedEvent(Guid bookId, Guid userId, Guid loanId)
    {
        BookId = bookId;
        UserId = userId;
        LoanId = loanId;
    }

    public BookReturnedEvent(Guid bookId, Guid userId, Guid loanId, DateTimeOffset occurredAt)
        : base(occurredAt)
    {
        BookId = bookId;
        UserId = userId;
        LoanId = loanId;
    }
}
