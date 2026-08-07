using MediatR;

namespace MiniLibrary.Application.Gamification.Commands.EvaluateBadges;

/// <summary>
/// Command to evaluate and award badges for a member after a qualifying event
/// (book return or review creation). Idempotent: badges are awarded exactly once.
/// </summary>
public sealed record EvaluateBadgesCommand(Guid UserId) : IRequest<Unit>;
