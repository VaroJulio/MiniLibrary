using MediatR;
using MiniLibrary.Application.Books.DTOs;

namespace MiniLibrary.Application.Books.Queries.SemanticSearch;

/// <summary>
/// Query to perform semantic (AI-powered) search using natural language.
/// The query text is embedded and compared via cosine similarity against stored book embeddings.
/// </summary>
public sealed record SemanticSearchQuery : IRequest<SemanticSearchResponse>
{
    /// <summary>Natural language search query. Truncated to 500 chars if longer.</summary>
    public string Query { get; init; } = string.Empty;

    /// <summary>Maximum number of results to return (default: 20).</summary>
    public int MaxResults { get; init; } = 20;

    /// <summary>Minimum relevance score threshold (default: 0.3).</summary>
    public float Threshold { get; init; } = 0.3f;
}
