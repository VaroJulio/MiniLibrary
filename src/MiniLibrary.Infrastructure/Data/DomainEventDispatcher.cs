using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Events;

namespace MiniLibrary.Infrastructure.Data;

/// <summary>
/// EF Core SaveChanges interceptor that publishes domain events via MediatR
/// after entities have been successfully persisted. Events are cleared from
/// entities after publishing to prevent duplicate dispatch.
/// </summary>
public sealed class DomainEventDispatcher : SaveChangesInterceptor
{
    private readonly IMediator _mediator;

    public DomainEventDispatcher(IMediator mediator)
    {
        _mediator = mediator;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is null)
            return result;

        await DispatchDomainEventsAsync(eventData.Context, cancellationToken);

        return result;
    }

    private async Task DispatchDomainEventsAsync(DbContext context, CancellationToken ct)
    {
        var entities = context.ChangeTracker
            .Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count > 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        // Clear events before publishing to prevent re-entrancy issues
        foreach (var entity in entities)
        {
            entity.ClearDomainEvents();
        }

        // Publish each event as a MediatR notification
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent, ct);
        }
    }
}
