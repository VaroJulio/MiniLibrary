using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for ReviewVote entities.
/// </summary>
public interface IReviewVoteRepository
{
    Task<ReviewVote?> GetByUserAndRatingAsync(Guid userId, Guid ratingId, CancellationToken ct);
    Task AddAsync(ReviewVote vote, CancellationToken ct);
}
