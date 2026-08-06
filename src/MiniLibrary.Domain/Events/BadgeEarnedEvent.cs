using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Domain.Events;

/// <summary>Raised when a member earns a gamification badge. Triggers notification generation.</summary>
public sealed class BadgeEarnedEvent : DomainEvent
{
    public Guid UserId { get; }
    public BadgeType BadgeType { get; }

    public BadgeEarnedEvent(Guid userId, BadgeType badgeType)
    {
        UserId = userId;
        BadgeType = badgeType;
    }
}
