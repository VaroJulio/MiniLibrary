using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Books.DTOs;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.Queries.SemanticSearch;

/// <summary>
/// Handles the SemanticSearchQuery by:
/// 1. Generating an embedding for the user's natural language query
/// 2. Comparing via cosine similarity against stored book embeddings
/// 3. Falling back to text search if embedding generation fails
/// </summary>
public sealed class SemanticSearchQueryHandler : IRequestHandler<SemanticSearchQuery, SemanticSearchResponse>
{
    private readonly IEmbeddingService _embeddingService;
    private readonly IBookRepository _bookRepository;
    private readonly ILogger<SemanticSearchQueryHandler> _logger;

    private const int MaxQueryLength = 500;

    public SemanticSearchQueryHandler(
        IEmbeddingService embeddingService,
        IBookRepository bookRepository,
        ILogger<SemanticSearchQueryHandler> logger)
    {
        _embeddingService = embeddingService;
        _bookRepository = bookRepository;
        _logger = logger;
    }

    public async Task<SemanticSearchResponse> Handle(SemanticSearchQuery request, CancellationToken cancellationToken)
    {
        // Truncate query to 500 chars without notification (Req 4.6)
        var queryText = request.Query.Length > MaxQueryLength
            ? request.Query[..MaxQueryLength]
            : request.Query;

        // Generate embedding for the query
        var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(queryText, cancellationToken);

        // If embedding generation failed, fall back to text search (Req 4.8)
        if (queryEmbedding is null)
        {
            _logger.LogInformation("Semantic search falling back to text search for query: {Query}", queryText[..Math.Min(50, queryText.Length)]);
            return await FallbackToTextSearchAsync(queryText, request.MaxResults, cancellationToken);
        }

        // Perform similarity search
        var semanticResults = await _embeddingService.SearchSimilarAsync(
            queryEmbedding,
            request.MaxResults,
            request.Threshold,
            cancellationToken);

        // If no semantic results, fall back to text search
        if (semanticResults.Count == 0)
        {
            _logger.LogInformation("Semantic search found no results above threshold, falling back to text search.");
            return await FallbackToTextSearchAsync(queryText, request.MaxResults, cancellationToken);
        }

        // Load book details for the matched IDs
        var bookIds = semanticResults.Select(r => r.BookId).ToList();
        var results = new List<SemanticSearchResultItem>();

        foreach (var semanticResult in semanticResults)
        {
            var book = await _bookRepository.GetByIdAsync(semanticResult.BookId, cancellationToken);
            if (book is null || book.IsDeleted)
                continue;

            results.Add(new SemanticSearchResultItem(
                book.Id,
                book.Title,
                book.Author,
                book.ISBN,
                book.PublishedYear,
                book.Description,
                book.Category,
                book.Status.ToString(),
                book.AverageRating,
                book.TotalRatings,
                semanticResult.Score));
        }

        return new SemanticSearchResponse(results, UsedFallback: false);
    }

    private async Task<SemanticSearchResponse> FallbackToTextSearchAsync(
        string queryText,
        int maxResults,
        CancellationToken cancellationToken)
    {
        var criteria = new Domain.Common.SearchCriteria
        {
            Query = queryText,
            Page = 1,
            PageSize = maxResults
        };

        var pagedResult = await _bookRepository.SearchAsync(criteria, cancellationToken);

        var results = pagedResult.Items.Select(book => new SemanticSearchResultItem(
            book.Id,
            book.Title,
            book.Author,
            book.ISBN,
            book.PublishedYear,
            book.Description,
            book.Category,
            book.Status.ToString(),
            book.AverageRating,
            book.TotalRatings,
            RelevanceScore: 0f))
            .ToList();

        return new SemanticSearchResponse(results, UsedFallback: true);
    }
}
