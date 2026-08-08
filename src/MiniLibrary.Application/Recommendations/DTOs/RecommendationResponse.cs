namespace MiniLibrary.Application.Recommendations.DTOs;

/// <summary>
/// DTO representing a single book recommendation in API responses.
/// </summary>
public sealed record RecommendationResponse(
    Guid BookId,
    string Title,
    string Author,
    string Category,
    string Justification);
