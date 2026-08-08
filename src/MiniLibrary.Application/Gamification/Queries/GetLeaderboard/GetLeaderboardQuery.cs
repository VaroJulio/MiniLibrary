using MediatR;
using MiniLibrary.Application.Gamification.DTOs;

namespace MiniLibrary.Application.Gamification.Queries.GetLeaderboard;

/// <summary>
/// Query to retrieve the gamification leaderboard (top 10 by badge count). Cached 1 hour.
/// </summary>
public sealed record GetLeaderboardQuery : IRequest<List<LeaderboardEntry>>;
