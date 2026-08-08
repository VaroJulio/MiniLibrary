using System.Text.Json;
using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Application.Common;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for pagination metadata consistency and JSON serialization round-trip.
/// Property 10: Pagination Metadata Consistency — totalPages = ceil(T/S), hasNext/hasPrevious correctness.
/// Property 11: JSON Serialization Round-Trip — serialize to camelCase JSON, deserialize back, verify equivalence.
/// **Validates: Requirements 13.1, 13.2, 13.3, 14.1, 14.2, 14.3, 14.4**
/// </summary>
[Trait("Category", "Property")]
public class PaginationSerializationProperties
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    // ── Property 10a: TotalPages = ceil(TotalCount / PageSize) ───────────────────

    /// <summary>
    /// For any (totalCount, pageSize), totalPages must equal ceil(totalCount / pageSize).
    /// **Validates: Requirements 13.3**
    /// </summary>
    [Property(MaxTest = 200)]
    [Trait("Category", "Property")]
    public Property TotalPages_EqualsCeilingDivision()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, 1000)),
            Arb.From(Gen.Choose(1, 100)),
            (totalCount, pageSize) =>
            {
                var response = PagedResponse<string>.Create([], totalCount, 1, pageSize);
                var expected = (int)Math.Ceiling((double)totalCount / pageSize);
                return response.Pagination.TotalPages == expected;
            });
    }

    // ── Property 10b: HasNext is true iff currentPage < totalPages ───────────────

    /// <summary>
    /// HasNext must be true when currentPage < totalPages, false otherwise.
    /// **Validates: Requirements 13.2**
    /// </summary>
    [Property(MaxTest = 200)]
    [Trait("Category", "Property")]
    public Property HasNext_TrueIffCurrentPageLessThanTotalPages()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, 500)),
            Arb.From(Gen.Choose(1, 50)),
            Arb.From(Gen.Choose(1, 30)),
            (totalCount, pageSize, currentPage) =>
            {
                var response = PagedResponse<string>.Create([], totalCount, currentPage, pageSize);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                var expectedHasNext = currentPage < totalPages;
                return response.Pagination.HasNext == expectedHasNext;
            });
    }

    // ── Property 10c: HasPrevious is true iff currentPage > 1 ────────────────────

    /// <summary>
    /// HasPrevious must be true when currentPage > 1, false otherwise.
    /// **Validates: Requirements 13.2**
    /// </summary>
    [Property(MaxTest = 200)]
    [Trait("Category", "Property")]
    public Property HasPrevious_TrueIffCurrentPageGreaterThanOne()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, 500)),
            Arb.From(Gen.Choose(1, 50)),
            Arb.From(Gen.Choose(1, 30)),
            (totalCount, pageSize, currentPage) =>
            {
                var response = PagedResponse<string>.Create([], totalCount, currentPage, pageSize);
                var expectedHasPrevious = currentPage > 1;
                return response.Pagination.HasPrevious == expectedHasPrevious;
            });
    }

    // ── Property 10d: Page 1 never has HasPrevious ───────────────────────────────

    /// <summary>
    /// Page 1 must never have HasPrevious = true.
    /// **Validates: Requirements 13.1**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property PageOne_NeverHasPrevious()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, 1000)),
            Arb.From(Gen.Choose(1, 100)),
            (totalCount, pageSize) =>
            {
                var response = PagedResponse<string>.Create([], totalCount, 1, pageSize);
                return !response.Pagination.HasPrevious;
            });
    }

    // ── Property 10e: Empty result has correct metadata ──────────────────────────

    /// <summary>
    /// When totalCount is 0, totalPages is 0, hasNext is false, hasPrevious depends on page.
    /// **Validates: Requirements 13.4**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property EmptyResult_HasZeroTotalPages()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(1, 100)),
            pageSize =>
            {
                var response = PagedResponse<string>.Create([], 0, 1, pageSize);
                return response.Pagination.TotalPages == 0
                    && !response.Pagination.HasNext
                    && !response.Pagination.HasPrevious
                    && response.Pagination.TotalCount == 0;
            });
    }

    // ── Property 11a: JSON serialization round-trip preserves PaginationMetadata ──

    /// <summary>
    /// Serializing PaginationMetadata to JSON (camelCase) and deserializing back
    /// produces an equivalent object.
    /// **Validates: Requirements 14.1, 14.3**
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property JsonRoundTrip_PreservesPaginationMetadata()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(0, 1000)),
            Arb.From(Gen.Choose(1, 100)),
            Arb.From(Gen.Choose(1, 50)),
            (totalCount, pageSize, currentPage) =>
            {
                var response = PagedResponse<string>.Create(
                    new List<string> { "item1", "item2" }, totalCount, currentPage, pageSize);

                var json = JsonSerializer.Serialize(response, JsonOptions);
                var deserialized = JsonSerializer.Deserialize<PagedResponse<string>>(json, JsonOptions);

                if (deserialized is null) return false;

                return deserialized.Pagination.TotalCount == response.Pagination.TotalCount
                    && deserialized.Pagination.PageSize == response.Pagination.PageSize
                    && deserialized.Pagination.CurrentPage == response.Pagination.CurrentPage
                    && deserialized.Pagination.TotalPages == response.Pagination.TotalPages
                    && deserialized.Pagination.HasNext == response.Pagination.HasNext
                    && deserialized.Pagination.HasPrevious == response.Pagination.HasPrevious
                    && deserialized.Data.Count == response.Data.Count;
            });
    }

    // ── Property 11b: JSON uses camelCase property names ─────────────────────────

    /// <summary>
    /// Serialized JSON must use camelCase property names (data, pagination, totalCount, etc).
    /// **Validates: Requirements 14.4**
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property JsonOutput_UsesCamelCase()
    {
        return Prop.ForAll(
            Arb.From(Gen.Choose(1, 100)),
            Arb.From(Gen.Choose(1, 20)),
            (totalCount, pageSize) =>
            {
                var response = PagedResponse<string>.Create(
                    new List<string> { "test" }, totalCount, 1, pageSize);

                var json = JsonSerializer.Serialize(response, JsonOptions);

                return json.Contains("\"data\"")
                    && json.Contains("\"pagination\"")
                    && json.Contains("\"totalCount\"")
                    && json.Contains("\"pageSize\"")
                    && json.Contains("\"currentPage\"")
                    && json.Contains("\"totalPages\"")
                    && json.Contains("\"hasNext\"")
                    && json.Contains("\"hasPrevious\"");
            });
    }
}
