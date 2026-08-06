using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for WishlistEntry entities.
/// </summary>
public interface IWishlistRepository
{
    Task<PagedResult<WishlistEntry>> GetUserWishlistAsync(Guid userId, PaginationParams paging, CancellationToken ct);
    Task<WishlistEntry?> GetEntryAsync(Guid userId, Guid bookId, CancellationToken ct);
    Task<int> GetUserWishlistCountAsync(Guid userId, CancellationToken ct);
    Task<List<WishlistEntry>> GetBookWatchersAsync(Guid bookId, CancellationToken ct);
    Task AddAsync(WishlistEntry entry, CancellationToken ct);
    Task DeleteAsync(WishlistEntry entry, CancellationToken ct);
}
