using MediatR;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Rankings.Queries.GetReaderRankings;

/// <summary>
/// Query to retrieve reader rankings by loan return count in a given period.
/// Includes the requesting member's own position. Cached for 1 hour.
/// </summary>
public sealed record GetReaderRankingsQuery : IRequest<ReaderRankingsResponse>
{
    /// <summary>Period filter: 30d, 90d, 12m, all (default: all).</summary>
    public string Period { get; init; } = "all";

    /// <summary>The requesting user's ID (to determine their position).</summary>
    public Guid? RequestingUserId { get; init; }
}
