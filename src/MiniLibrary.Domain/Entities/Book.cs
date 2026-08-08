using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Events;

namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Aggregate root representing a book in the library catalog.
/// </summary>
public class Book : Entity
{
    public string Title { get; private set; } = string.Empty;
    public string Author { get; private set; } = string.Empty;
    public string ISBN { get; private set; } = string.Empty;
    public int PublishedYear { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public BookStatus Status { get; private set; }
    public decimal AverageRating { get; private set; }
    public int TotalRatings { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Concurrency token for optimistic locking (Req 11.5)
    public byte[]? RowVersion { get; private set; }

    // Navigation properties
    public ICollection<BookLoan> Loans { get; private set; } = [];
    public ICollection<Rating> Ratings { get; private set; } = [];
    public ICollection<WishlistEntry> WishlistEntries { get; private set; } = [];
    public BookEmbedding? Embedding { get; private set; }

    // Required by EF Core
    private Book() { }

    public static Book Create(
        string title,
        string author,
        string isbn,
        int publishedYear,
        string description,
        string category)
    {
        var book = new Book
        {
            Title = title,
            Author = author,
            ISBN = isbn,
            PublishedYear = publishedYear,
            Description = description,
            Category = category,
            Status = BookStatus.Available,
            AverageRating = 0m,
            TotalRatings = 0,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        book.RaiseDomainEvent(new BookCreatedEvent(book.Id));

        return book;
    }

    public void Update(
        string title,
        string author,
        string isbn,
        int publishedYear,
        string description,
        string category)
    {
        Title = title;
        Author = author;
        ISBN = isbn;
        PublishedYear = publishedYear;
        Description = description;
        Category = category;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new BookUpdatedEvent(Id));
    }

    public void CheckOut()
    {
        if (Status != BookStatus.Available)
            throw new InvalidOperationException($"Book '{Title}' is not available for checkout.");

        Status = BookStatus.CheckedOut;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MakeAvailable()
    {
        Status = BookStatus.Available;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        IsDeleted = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRatingStats(decimal averageRating, int totalRatings)
    {
        AverageRating = averageRating;
        TotalRatings = totalRatings;
        UpdatedAt = DateTime.UtcNow;
    }
}
