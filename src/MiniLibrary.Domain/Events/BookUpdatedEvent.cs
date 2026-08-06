namespace MiniLibrary.Domain.Events;

/// <summary>Raised when a book's metadata is updated. Triggers embedding regeneration.</summary>
public record BookUpdatedEvent(Guid BookId);
