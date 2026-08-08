using MiniLibrary.Domain.Events;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity representing a member's rating and optional review text for a book.
/// Each rating is tied to a specific loan cycle (one rating per loan).
/// </summary>
public class Rating : Entity
{
    public Guid BookId { get; private set; }
    public Guid UserId { get; private set; }

    /// <summary>The loan this rating is associated with (nullable for legacy ratings).</summary>
    public Guid? LoanId { get; private set; }

    /// <summary>Score between 1 and 5 inclusive.</summary>
    public int Score { get; private set; }

    public string ReviewText { get; private set; } = string.Empty;
    public int UsefulVotes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation properties
    public Book Book { get; private set; } = null!;
    public User User { get; private set; } = null!;
    public BookLoan? Loan { get; private set; }
    public ICollection<ReviewVote> Votes { get; private set; } = [];

    // Required by EF Core
    private Rating() { }

    public static Rating Create(Guid bookId, Guid userId, int score, string reviewText, Guid? loanId = null)
    {
        var rating = new Rating
        {
            BookId = bookId,
            UserId = userId,
            LoanId = loanId,
            Score = score,
            ReviewText = reviewText,
            UsefulVotes = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        rating.RaiseDomainEvent(new RatingCreatedEvent(rating.Id, bookId));

        return rating;
    }

    public void Update(int score, string reviewText)
    {
        Score = score;
        ReviewText = reviewText;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new RatingCreatedEvent(Id, BookId));
    }

    public void IncrementUsefulVotes()
    {
        UsefulVotes++;
        UpdatedAt = DateTime.UtcNow;
    }
}
