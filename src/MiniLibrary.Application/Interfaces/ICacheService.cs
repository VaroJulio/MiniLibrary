namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for caching frequently accessed data (recommendations, rankings).
/// </summary>
public interface ICacheService
{
    /// <summary>Retrieves a cached value by key. Returns null if not found or expired.</summary>
    Task<T?> GetAsync<T>(string key, CancellationToken ct);

    /// <summary>Stores a value in cache with the specified expiration duration.</summary>
    Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct);

    /// <summary>Removes a cached entry by key.</summary>
    Task InvalidateAsync(string key, CancellationToken ct);
}
