using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for AI-powered book recommendations.
/// </summary>
public interface IRecommendationService
{
    /// <summary>
    /// Generates personalized book recommendations for a member based on their loan history
    /// and the available catalog. Returns 1–10 results with justifications.
    /// Falls back to popular books if the AI service is unavailable.
    /// </summary>
    Task<List<RecommendationResult>> GetRecommendationsAsync(
        Guid userId,
        List<BookLoan> history,
        List<Book> catalog,
        CancellationToken ct);
}
