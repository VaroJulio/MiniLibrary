using MediatR;
using MiniLibrary.Application.Recommendations.DTOs;

namespace MiniLibrary.Application.Recommendations.Queries.GetRecommendations;

/// <summary>
/// Query to retrieve personalized book recommendations for the authenticated member.
/// Results are cached per member for 1 hour.
/// </summary>
public sealed record GetRecommendationsQuery : IRequest<List<RecommendationResponse>>
{
    /// <summary>The authenticated member's user ID.</summary>
    public Guid UserId { get; init; }
}
