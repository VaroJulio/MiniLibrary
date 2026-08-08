namespace MiniLibrary.Domain.Common;

/// <summary>
/// A single AI-generated book recommendation with its justification.
/// </summary>
public sealed record RecommendationResult(
    Guid BookId,
    string Title,
    string Author,
    string Category,
    string Justification);
