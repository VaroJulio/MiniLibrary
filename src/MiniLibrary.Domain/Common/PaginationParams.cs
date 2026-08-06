namespace MiniLibrary.Domain.Common;

/// <summary>
/// Offset-based pagination parameters for list queries.
/// </summary>
public sealed record PaginationParams
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public int Page { get; init; } = DefaultPage;
    public int PageSize { get; init; } = DefaultPageSize;

    public PaginationParams() { }

    public PaginationParams(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }
}
