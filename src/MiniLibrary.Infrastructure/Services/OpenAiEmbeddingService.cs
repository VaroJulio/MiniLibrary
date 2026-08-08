using System.ClientModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Common;
using MiniLibrary.Infrastructure.Configuration;
using MiniLibrary.Infrastructure.Data;
using OpenAI;
using OpenAI.Embeddings;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// OpenAI-backed implementation of <see cref="IEmbeddingService"/>.
/// Uses text-embedding-3-small to generate vector embeddings and performs
/// cosine similarity search against stored BookEmbeddings.
/// Implements a 3-second timeout with graceful fallback (returns null).
/// </summary>
public sealed class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly EmbeddingClient _embeddingClient;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<OpenAiEmbeddingService> _logger;
    private readonly int _timeoutSeconds;

    public OpenAiEmbeddingService(
        IOptions<OpenAiOptions> options,
        AppDbContext dbContext,
        ILogger<OpenAiEmbeddingService> logger)
    {
        var config = options.Value;
        _timeoutSeconds = config.TimeoutSeconds;
        _dbContext = dbContext;
        _logger = logger;

        var openAiClient = new OpenAIClient(config.ApiKey);
        _embeddingClient = openAiClient.GetEmbeddingClient(config.EmbeddingModel);
    }

    /// <inheritdoc />
    public async Task<float[]?> GenerateEmbeddingAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

            ClientResult<OpenAIEmbedding> result = await _embeddingClient
                .GenerateEmbeddingAsync(text, cancellationToken: timeoutCts.Token);

            ReadOnlyMemory<float> vector = result.Value.ToFloats();
            return vector.ToArray();
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("OpenAI embedding request timed out after {Timeout}s. Falling back to text search.", _timeoutSeconds);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate embedding from OpenAI. Falling back to text search.");
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<List<SemanticResult>> SearchSimilarAsync(
        float[] queryEmbedding,
        int maxResults,
        float threshold,
        CancellationToken ct)
    {
        // Load all embeddings from the database (for small-to-medium catalogs).
        // For production at scale, a vector database would be used instead.
        var embeddings = await _dbContext.BookEmbeddings
            .AsNoTracking()
            .ToListAsync(ct);

        var results = new List<SemanticResult>();

        foreach (var embedding in embeddings)
        {
            var storedVector = DeserializeVector(embedding.Vector);
            if (storedVector.Length == 0)
                continue;

            var score = CosineSimilarity(queryEmbedding, storedVector);
            if (score >= threshold)
            {
                results.Add(new SemanticResult(embedding.BookId, score));
            }
        }

        return results
            .OrderByDescending(r => r.Score)
            .Take(maxResults)
            .ToList();
    }

    /// <summary>
    /// Serializes a float[] vector to a byte[] for storage in the BookEmbedding table.
    /// </summary>
    public static byte[] SerializeVector(float[] vector)
    {
        var bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    /// <summary>
    /// Deserializes a byte[] back to a float[] vector.
    /// </summary>
    public static float[] DeserializeVector(byte[] bytes)
    {
        if (bytes.Length == 0)
            return [];

        var vector = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, vector, 0, bytes.Length);
        return vector;
    }

    /// <summary>
    /// Computes cosine similarity between two vectors.
    /// Returns a value between -1.0 and 1.0 where 1.0 indicates identical direction.
    /// </summary>
    private static float CosineSimilarity(float[] a, float[] b)
    {
        if (a.Length != b.Length || a.Length == 0)
            return 0f;

        float dotProduct = 0f;
        float magnitudeA = 0f;
        float magnitudeB = 0f;

        for (int i = 0; i < a.Length; i++)
        {
            dotProduct += a[i] * b[i];
            magnitudeA += a[i] * a[i];
            magnitudeB += b[i] * b[i];
        }

        var denominator = MathF.Sqrt(magnitudeA) * MathF.Sqrt(magnitudeB);
        if (denominator == 0f)
            return 0f;

        return dotProduct / denominator;
    }
}
