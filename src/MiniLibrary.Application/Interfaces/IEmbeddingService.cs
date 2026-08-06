using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Interfaces;

/// <summary>
/// Service contract for generating and querying vector embeddings for semantic search.
/// </summary>
public interface IEmbeddingService
{
    /// <summary>
    /// Generates a vector embedding for the given text using an external AI model.
    /// Returns null if the external service is unavailable or times out.
    /// </summary>
    Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct);

    /// <summary>
    /// Searches for books similar to the provided query embedding using cosine similarity.
    /// Results are filtered by the specified similarity threshold.
    /// </summary>
    Task<List<SemanticResult>> SearchSimilarAsync(float[] queryEmbedding, int maxResults, float threshold, CancellationToken ct);
}
