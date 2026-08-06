namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity representing a book in a member's wish list.
/// </summary>
public class WishlistEntry : Entity
{
    public Guid BookId { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime AddedAt { get; private set; }

    // Navigation properties
    public Book Book { get; private set; } = null!;
    public User User { get; private set; } = null!;

    // Required by EF Core
    private WishlistEntry() { }

    public static WishlistEntry Create(Guid bookId, Guid userId)
    {
        return new WishlistEntry
        {
            BookId = bookId,
            UserId = userId,
            AddedAt = DateTime.UtcNow
        };
    }
}
