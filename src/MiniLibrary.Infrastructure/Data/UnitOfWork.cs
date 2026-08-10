using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Infrastructure.Data;

/// <summary>
/// EF Core implementation of <see cref="IUnitOfWork"/>.
/// Wraps AppDbContext.SaveChangesAsync to flush all pending changes atomically.
/// Registered as Scoped — shares the same DbContext instance as repositories.
/// </summary>
public sealed class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> CommitAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
