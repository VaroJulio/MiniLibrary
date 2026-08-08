using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for BookEmbedding entities.
/// </summary>
public interface IBookEmbeddingRepository
{
    Task<BookEmbedding?> GetByBookIdAsync(Guid bookId, CancellationToken ct);
    Task AddAsync(BookEmbedding embedding, CancellationToken ct);
    Task UpdateAsync(BookEmbedding embedding, CancellationToken ct);
}
