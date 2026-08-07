namespace MiniLibrary.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the OpenAI embedding service.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>OpenAI API key for authentication.</summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>Model name for embedding generation (default: text-embedding-3-small).</summary>
    public string EmbeddingModel { get; set; } = "text-embedding-3-small";

    /// <summary>Timeout in seconds for embedding API calls (default: 3).</summary>
    public int TimeoutSeconds { get; set; } = 3;
}
