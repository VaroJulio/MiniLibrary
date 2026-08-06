namespace MiniLibrary.Domain.Events;

/// <summary>Raised when a new book is added to the catalog. Triggers embedding generation.</summary>
public record BookCreatedEvent(Guid BookId);
