namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a new book is created in the catalog.
/// Used to trigger embedding generation for semantic search.
/// </summary>
public sealed class BookCreatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    public string Author { get; }
    public string Description { get; }

    public BookCreatedEvent(Guid bookId, string title, string author, string description)
    {
        BookId = bookId;
        Title = title;
        Author = author;
        Description = description;
    }

    public BookCreatedEvent(Guid bookId, string title, string author, string description, DateTimeOffset occurredAt)
        : base(occurredAt)
    {
        BookId = bookId;
        Title = title;
        Author = author;
        Description = description;
    }
}
