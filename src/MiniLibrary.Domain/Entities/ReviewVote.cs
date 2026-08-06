namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity tracking a member's "useful" vote on a review.
/// One vote per member per review is enforced at the database level.
/// </summary>
public class ReviewVote : Entity
{
    public Guid RatingId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Rating Rating { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // Required by EF Core
    private ReviewVote() { }

    public static ReviewVote Create(Guid ratingId, Guid userId)
    {
        return new ReviewVote
        {
            RatingId = ratingId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };
    }
}
