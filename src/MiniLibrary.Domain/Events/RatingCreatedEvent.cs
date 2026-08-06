namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a rating is created or updated.
/// Triggers ranking cache invalidation.
/// </summary>
public record RatingCreatedEvent(Guid RatingId, Guid BookId);
