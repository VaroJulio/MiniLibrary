namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a rating is created or updated.
/// Triggers ranking cache invalidation.
/// </summary>
public sealed class RatingCreatedEvent : DomainEvent
{
    public Guid RatingId { get; }
    public Guid BookId { get; }

    public RatingCreatedEvent(Guid ratingId, Guid bookId)
    {
        RatingId = ratingId;
        BookId = bookId;
    }
}
