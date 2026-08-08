using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for Rating entities.
/// </summary>
public interface IRatingRepository
{
    Task<Rating?> GetByUserAndBookAsync(Guid userId, Guid bookId, CancellationToken ct);
    Task<Rating?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<PagedResult<Rating>> GetBookRatingsAsync(Guid bookId, PaginationParams paging, CancellationToken ct);
    Task<List<Rating>> GetRecentBookRatingsAsync(Guid bookId, int count, CancellationToken ct);
    Task<(decimal Average, int Count)> CalculateBookAverageAsync(Guid bookId, CancellationToken ct);
    Task<int> GetUserRatingCountAsync(Guid userId, CancellationToken ct);
    Task<int> GetUserUsefulReviewCountAsync(Guid userId, int minVotes, CancellationToken ct);
    Task AddAsync(Rating rating, CancellationToken ct);
    Task UpdateAsync(Rating rating, CancellationToken ct);
    Task DeleteAsync(Rating rating, CancellationToken ct);
}
