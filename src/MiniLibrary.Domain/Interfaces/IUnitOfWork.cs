namespace MiniLibrary.Domain.Interfaces;

/// <summary>
/// Unit of Work abstraction for coordinating atomic persistence of changes
/// across multiple repositories within a single transaction.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes to the database in a single transaction.
    /// Call this once at the end of a command handler after all repository
    /// operations (Add, Update, Delete) have been invoked.
    /// </summary>
    /// <returns>The number of entities written to the database.</returns>
    Task<int> CommitAsync(CancellationToken ct = default);
}
