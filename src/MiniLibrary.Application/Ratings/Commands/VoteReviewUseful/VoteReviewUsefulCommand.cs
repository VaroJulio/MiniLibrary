using MediatR;

namespace MiniLibrary.Application.Ratings.Commands.VoteReviewUseful;

/// <summary>
/// Command to vote a review as useful. One vote per member per review.
/// Self-votes are rejected with 403.
/// </summary>
public sealed record VoteReviewUsefulCommand(Guid RatingId, Guid UserId) : IRequest<Unit>;
