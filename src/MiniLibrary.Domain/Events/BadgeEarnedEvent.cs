namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a member earns a new badge.
/// Used to trigger notification generation (in-app and email).
/// </summary>
public sealed class BadgeEarnedEvent : DomainEvent
{
    public Guid UserId { get; }
    public string BadgeType { get; }

    public BadgeEarnedEvent(Guid userId, string badgeType)
    {
        UserId = userId;
        BadgeType = badgeType;
    }

    public BadgeEarnedEvent(Guid userId, string badgeType, DateTimeOffset occurredAt)
        : base(occurredAt)
    {
        UserId = userId;
        BadgeType = badgeType;
    }
}
