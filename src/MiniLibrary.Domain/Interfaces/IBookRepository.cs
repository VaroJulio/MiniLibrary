using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for the Book aggregate.
/// </summary>
public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Book?> GetByIsbnAsync(string isbn, CancellationToken ct);
    Task<PagedResult<Book>> SearchAsync(SearchCriteria criteria, CancellationToken ct);
    Task AddAsync(Book book, CancellationToken ct);
    Task UpdateAsync(Book book, CancellationToken ct);
    Task DeleteAsync(Book book, CancellationToken ct);
}
