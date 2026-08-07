using MediatR;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Rankings.Queries.GetBookRankings;

/// <summary>
/// Query to retrieve book rankings. Only books with >= 3 ratings are included.
/// Supports filters (category, year range, availability) and sorting.
/// Results cached for 15 minutes.
/// </summary>
public sealed record GetBookRankingsQuery : IRequest<List<BookRankingItem>>
{
    /// <summary>Filter by category.</summary>
    public string? Category { get; init; }

    /// <summary>Filter by minimum publication year.</summary>
    public int? YearFrom { get; init; }

    /// <summary>Filter by maximum publication year.</summary>
    public int? YearTo { get; init; }

    /// <summary>Filter by availability (true = Available only).</summary>
    public bool? AvailableOnly { get; init; }

    /// <summary>Sort field: averageRating (default), totalRatings, totalLoans, publishedYear.</summary>
    public string SortBy { get; init; } = "averageRating";

    /// <summary>Sort descending (default: true).</summary>
    public bool SortDescending { get; init; } = true;
}
