namespace MiniLibrary.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// All domain events must implement this interface.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// The timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
