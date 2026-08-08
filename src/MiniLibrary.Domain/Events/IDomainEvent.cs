using MediatR;

namespace MiniLibrary.Domain.Events;

/// <summary>
/// Marker interface for domain events.
/// All domain events must implement this interface.
/// Extends INotification for MediatR-based event dispatching.
/// </summary>
public interface IDomainEvent : INotification
{
    /// <summary>
    /// The timestamp when the event occurred.
    /// </summary>
    DateTimeOffset OccurredAt { get; }
}
