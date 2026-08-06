namespace MiniLibrary.Domain.Events;

/// <summary>
/// Base class for all domain events. Provides a default OccurredAt timestamp
/// set to the time of event creation.
/// </summary>
public abstract class DomainEvent : IDomainEvent
{
    /// <inheritdoc />
    public DateTimeOffset OccurredAt { get; }

    protected DomainEvent()
    {
        OccurredAt = DateTimeOffset.UtcNow;
    }

    protected DomainEvent(DateTimeOffset occurredAt)
    {
        OccurredAt = occurredAt;
    }
}
