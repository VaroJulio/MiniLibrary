namespace MiniLibrary.Domain.Events;

/// <summary>Raised when a book's metadata is updated. Triggers embedding regeneration.</summary>
public sealed class BookUpdatedEvent : DomainEvent
{
    public Guid BookId { get; }

    public BookUpdatedEvent(Guid bookId)
    {
        BookId = bookId;
    }
}
