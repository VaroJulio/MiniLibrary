using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using MiniLibrary.Infrastructure.Services;

namespace MiniLibrary.UnitTests.Services;

public class MemoryCacheServiceTests : IDisposable
{
    private readonly MemoryCache _memoryCache;
    private readonly MemoryCacheService _sut;

    public MemoryCacheServiceTests()
    {
        _memoryCache = new MemoryCache(Options.Create(new MemoryCacheOptions()));
        _sut = new MemoryCacheService(_memoryCache);
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_WhenKeyNotFound()
    {
        var result = await _sut.GetAsync<string>("nonexistent-key", CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task SetAsync_StoresValue_ThatCanBeRetrieved()
    {
        var key = "test-key";
        var value = "test-value";

        await _sut.SetAsync(key, value, TimeSpan.FromMinutes(5), CancellationToken.None);
        var result = await _sut.GetAsync<string>(key, CancellationToken.None);

        result.Should().Be(value);
    }

    [Fact]
    public async Task SetAsync_StoresComplexObject_ThatCanBeRetrieved()
    {
        var key = "complex-key";
        var value = new TestDto("Book Title", 42);

        await _sut.SetAsync(key, value, TimeSpan.FromMinutes(10), CancellationToken.None);
        var result = await _sut.GetAsync<TestDto>(key, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Title.Should().Be("Book Title");
        result.Count.Should().Be(42);
    }

    [Fact]
    public async Task InvalidateAsync_RemovesEntry_FromCache()
    {
        var key = "invalidate-key";
        await _sut.SetAsync(key, "some-value", TimeSpan.FromMinutes(5), CancellationToken.None);

        await _sut.InvalidateAsync(key, CancellationToken.None);
        var result = await _sut.GetAsync<string>(key, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task InvalidateAsync_DoesNotThrow_WhenKeyDoesNotExist()
    {
        var act = () => _sut.InvalidateAsync("missing-key", CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SetAsync_OverwritesExistingValue_WithSameKey()
    {
        var key = "overwrite-key";

        await _sut.SetAsync(key, "first", TimeSpan.FromMinutes(5), CancellationToken.None);
        await _sut.SetAsync(key, "second", TimeSpan.FromMinutes(5), CancellationToken.None);
        var result = await _sut.GetAsync<string>(key, CancellationToken.None);

        result.Should().Be("second");
    }

    [Fact]
    public async Task GetAsync_ReturnsNull_AfterExpirationElapses()
    {
        var key = "expiring-key";
        // Use a very short expiration
        await _sut.SetAsync(key, "value", TimeSpan.FromMilliseconds(1), CancellationToken.None);

        // Wait for expiration
        await Task.Delay(50);
        var result = await _sut.GetAsync<string>(key, CancellationToken.None);

        result.Should().BeNull();
    }

    public void Dispose()
    {
        _memoryCache.Dispose();
    }

    private record TestDto(string Title, int Count);
}
