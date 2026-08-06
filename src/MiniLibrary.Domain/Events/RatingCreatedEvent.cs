namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a member creates or updates a rating for a book.
/// Used to trigger ranking cache invalidation.
/// </summary>
public sealed class RatingCreatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public Guid UserId { get; }
    public int Score { get; }

    public RatingCreatedEvent(Guid bookId, Guid userId, int score)
    {
        BookId = bookId;
        UserId = userId;
        Score = score;
    }

    public RatingCreatedEvent(Guid bookId, Guid userId, int score, DateTimeOffset occurredAt)
        : base(occurredAt)
    {
        BookId = bookId;
        UserId = userId;
        Score = score;
    }
}
