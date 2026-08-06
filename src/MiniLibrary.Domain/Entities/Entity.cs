using MiniLibrary.Domain.Events;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Base class for all domain entities, providing identity and domain event tracking.
/// </summary>
public abstract class Entity
{
    private readonly List<IDomainEvent> _domainEvents = [];

    public Guid Id { get; protected set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }
}
