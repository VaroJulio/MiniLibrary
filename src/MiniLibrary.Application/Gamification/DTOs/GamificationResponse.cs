namespace MiniLibrary.Application.Gamification.DTOs;

/// <summary>
/// DTO for a single earned badge.
/// </summary>
public sealed record BadgeResponse(
    string BadgeType,
    DateTime EarnedAt);

/// <summary>
/// DTO for badge progress (pending badge with progress indicator).
/// </summary>
public sealed record BadgeProgressResponse(
    string BadgeType,
    int CurrentCount,
    int RequiredCount,
    int ProgressPercent);

/// <summary>
/// DTO for the user's complete gamification profile.
/// </summary>
public sealed record UserBadgesResponse(
    List<BadgeResponse> EarnedBadges,
    List<BadgeProgressResponse> PendingBadges);

/// <summary>
/// DTO for a leaderboard entry.
/// </summary>
public sealed record LeaderboardEntry(
    int Position,
    Guid UserId,
    string Name,
    int BadgeCount);
