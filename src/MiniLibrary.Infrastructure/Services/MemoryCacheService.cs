using Microsoft.Extensions.Caching.Memory;
using MiniLibrary.Application.Interfaces;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// In-memory implementation of <see cref="ICacheService"/> backed by <see cref="IMemoryCache"/>.
/// Supports configurable expiration per cache entry.
/// </summary>
public sealed class MemoryCacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public MemoryCacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public Task<T?> GetAsync<T>(string key, CancellationToken ct)
    {
        _cache.TryGetValue(key, out T? value);
        return Task.FromResult(value);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken ct)
    {
        var options = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        _cache.Set(key, value, options);
        return Task.CompletedTask;
    }

    public Task InvalidateAsync(string key, CancellationToken ct)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }
}
