namespace MiniLibrary.Domain.Entities;

/// <summary>
/// Entity storing the vector embedding for a book used in semantic search.
/// </summary>
public class BookEmbedding : Entity
{
    public Guid BookId { get; private set; }

    /// <summary>Binary-serialized float[] vector from OpenAI text-embedding-3-small.</summary>
    public byte[] Vector { get; private set; } = [];

    public DateTime GeneratedAt { get; private set; }

    // Navigation property
    public Book Book { get; private set; } = null!;

    // Required by EF Core
    private BookEmbedding() { }

    public static BookEmbedding Create(Guid bookId, byte[] vector)
    {
        return new BookEmbedding
        {
            BookId = bookId,
            Vector = vector,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public void Update(byte[] vector)
    {
        Vector = vector;
        GeneratedAt = DateTime.UtcNow;
    }
}
