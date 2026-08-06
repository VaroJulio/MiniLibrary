using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Domain.Events;

/// <summary>Raised when a member earns a gamification badge. Triggers notification generation.</summary>
public record BadgeEarnedEvent(Guid UserId, BadgeType BadgeType);
