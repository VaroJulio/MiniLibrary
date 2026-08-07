using MediatR;
using MiniLibrary.Application.Gamification.DTOs;

namespace MiniLibrary.Application.Gamification.Queries.GetUserBadges;

/// <summary>
/// Query to retrieve a user's earned badges and progress toward pending badges.
/// </summary>
public sealed record GetUserBadgesQuery(Guid UserId, bool IncludeProgress = true)
    : IRequest<UserBadgesResponse>;
