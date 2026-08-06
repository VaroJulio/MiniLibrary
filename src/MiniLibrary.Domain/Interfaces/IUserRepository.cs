using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Persistence contract for User aggregate.
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<User?> GetByExternalIdAsync(string externalId, string provider, CancellationToken ct);
    Task<PagedResult<User>> GetAllAsync(PaginationParams paging, CancellationToken ct);
    Task<int> GetAdminCountAsync(CancellationToken ct);
    Task AddAsync(User user, CancellationToken ct);
    Task UpdateAsync(User user, CancellationToken ct);
}
