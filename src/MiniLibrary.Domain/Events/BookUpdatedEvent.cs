namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when an existing book is updated in the catalog.
/// Used to trigger re-generation of the embedding for semantic search.
/// </summary>
public sealed class BookUpdatedEvent : DomainEvent
{
    public Guid BookId { get; }
    public string Title { get; }
    public string Author { get; }
    public string Description { get; }

    public BookUpdatedEvent(Guid bookId, string title, string author, string description)
    {
        BookId = bookId;
        Title = title;
        Author = author;
        Description = description;
    }

    public BookUpdatedEvent(Guid bookId, string title, string author, string description, DateTimeOffset occurredAt)
        : base(occurredAt)
    {
        BookId = bookId;
        Title = title;
        Author = author;
        Description = description;
    }
}
