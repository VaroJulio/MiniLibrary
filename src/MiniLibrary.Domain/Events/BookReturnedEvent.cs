namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a book loan is returned.
/// Triggers badge evaluation and wishlist availability alerts.
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
}
