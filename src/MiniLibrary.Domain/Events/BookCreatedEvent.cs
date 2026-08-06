namespace MiniLibrary.Domain.Events;

/// <summary>Raised when a new book is added to the catalog. Triggers embedding generation.</summary>
public sealed class BookCreatedEvent : DomainEvent
{
    public Guid BookId { get; }

    public BookCreatedEvent(Guid bookId)
    {
        BookId = bookId;
    }
}
