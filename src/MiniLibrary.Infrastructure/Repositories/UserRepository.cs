using Microsoft.EntityFrameworkCore;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of <see cref="IUserRepository"/>.
/// Soft-delete filtering is handled by the global query filter on the User entity.
/// </summary>
public sealed class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetByExternalIdAsync(string externalId, string provider, CancellationToken ct)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.ExternalId == externalId && u.Provider == provider, ct);
    }

    public async Task<PagedResult<User>> GetAllAsync(PaginationParams paging, CancellationToken ct)
    {
        var baseQuery = _context.Users.AsNoTracking();

        var totalCount = await baseQuery.CountAsync(ct);

        var items = await baseQuery
            .OrderBy(u => u.FullName)
            .Skip((paging.Page - 1) * paging.PageSize)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return new PagedResult<User>(items, totalCount, paging.Page, paging.PageSize);
    }

    public async Task<int> GetAdminCountAsync(CancellationToken ct)
    {
        return await _context.Users.AsNoTracking()
            .CountAsync(u => u.Role == UserRole.Admin, ct);
    }

    public async Task AddAsync(User user, CancellationToken ct)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public async Task UpdateAsync(User user, CancellationToken ct)
    {
        _context.Users.Update(user);
        await Task.CompletedTask;
    }
}
