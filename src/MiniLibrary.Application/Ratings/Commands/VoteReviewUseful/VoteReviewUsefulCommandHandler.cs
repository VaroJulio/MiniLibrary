using MediatR;
using Microsoft.EntityFrameworkCore;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Commands.VoteReviewUseful;

/// <summary>
/// Handles VoteReviewUsefulCommand:
/// - Validates rating exists
/// - Rejects self-votes (Req 20.9)
/// - Enforces one vote per member per review (Req 16.6)
/// - Increments useful vote count
/// </summary>
public sealed class VoteReviewUsefulCommandHandler : IRequestHandler<VoteReviewUsefulCommand, Unit>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IReviewVoteRepository _reviewVoteRepository;

    public VoteReviewUsefulCommandHandler(
        IRatingRepository ratingRepository,
        IReviewVoteRepository reviewVoteRepository)
    {
        _ratingRepository = ratingRepository;
        _reviewVoteRepository = reviewVoteRepository;
    }

    public async Task<Unit> Handle(VoteReviewUsefulCommand request, CancellationToken cancellationToken)
    {
        var rating = await _ratingRepository.GetByIdAsync(request.RatingId, cancellationToken);
        if (rating is null)
        {
            throw new NotFoundException("Rating", request.RatingId);
        }

        // Reject self-votes (Req 20.9)
        if (rating.UserId == request.UserId)
        {
            throw new ForbiddenException("You cannot vote on your own review.");
        }

        // Check if already voted (one vote per member per review)
        var existingVote = await _reviewVoteRepository.GetByUserAndRatingAsync(
            request.UserId, request.RatingId, cancellationToken);
        if (existingVote is not null)
        {
            throw new ConflictException("You have already voted on this review.");
        }

        // Create vote and increment counter
        var vote = ReviewVote.Create(request.RatingId, request.UserId);
        await _reviewVoteRepository.AddAsync(vote, cancellationToken);

        rating.IncrementUsefulVotes();
        await _ratingRepository.UpdateAsync(rating, cancellationToken);

        return Unit.Value;
    }
}
