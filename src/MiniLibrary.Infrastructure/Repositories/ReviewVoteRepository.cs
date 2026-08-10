using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IReviewVoteRepository"/>.
/// </summary>
public sealed class ReviewVoteRepository : IReviewVoteRepository
{
    private readonly AppDbContext _context;

    public ReviewVoteRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReviewVote?> GetByUserAndRatingAsync(Guid userId, Guid ratingId, CancellationToken ct)
    {
        return await _context.ReviewVotes
            .FirstOrDefaultAsync(v => v.UserId == userId && v.RatingId == ratingId, ct);
    }

    public async Task AddAsync(ReviewVote vote, CancellationToken ct)
    {
        await _context.ReviewVotes.AddAsync(vote, ct);
    }
}
