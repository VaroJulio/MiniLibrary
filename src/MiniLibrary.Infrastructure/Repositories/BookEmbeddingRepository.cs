using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IBookEmbeddingRepository"/>.
/// </summary>
public sealed class BookEmbeddingRepository : IBookEmbeddingRepository
{
    private readonly AppDbContext _context;

    public BookEmbeddingRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<BookEmbedding?> GetByBookIdAsync(Guid bookId, CancellationToken ct)
    {
        return await _context.BookEmbeddings
            .FirstOrDefaultAsync(e => e.BookId == bookId, ct);
    }

    public async Task AddAsync(BookEmbedding embedding, CancellationToken ct)
    {
        await _context.BookEmbeddings.AddAsync(embedding, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(BookEmbedding embedding, CancellationToken ct)
    {
        _context.BookEmbeddings.Update(embedding);
        await _context.SaveChangesAsync(ct);
    }
}
