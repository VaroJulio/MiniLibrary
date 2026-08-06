using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Events;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity representing a gamification badge earned by a member.
/// </summary>
public class Badge : Entity
{
    public Guid UserId { get; private set; }
    public BadgeType BadgeType { get; private set; }
    public DateTime EarnedAt { get; private set; }

    // Navigation property
    public User User { get; private set; } = null!;

    // Required by EF Core
    private Badge() { }

    public static Badge Create(Guid userId, BadgeType badgeType)
    {
        var badge = new Badge
        {
            UserId = userId,
            BadgeType = badgeType,
            EarnedAt = DateTime.UtcNow
        };

        badge.RaiseDomainEvent(new BadgeEarnedEvent(userId, badgeType));

        return badge;
    }
}
