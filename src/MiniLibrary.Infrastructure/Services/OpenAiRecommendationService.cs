using System.ClientModel;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Infrastructure.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace MiniLibrary.Infrastructure.Services;

/// <summary>
/// OpenAI-backed implementation of <see cref="IRecommendationService"/>.
/// Uses GPT-4o-mini to generate personalized book recommendations based on
/// a member's loan history and available catalog.
/// Implements a 10-second timeout with fallback to popular books.
/// For members with fewer than 3 loans, returns top borrowed books.
/// </summary>
public sealed class OpenAiRecommendationService : IRecommendationService
{
    private readonly ChatClient _chatClient;
    private readonly ILogger<OpenAiRecommendationService> _logger;
    private const int TimeoutSeconds = 10;
    private const int MinHistoryForAi = 3;
    private const int MaxRecommendations = 10;

    public OpenAiRecommendationService(
        IOptions<OpenAiOptions> options,
        ILogger<OpenAiRecommendationService> logger)
    {
        _logger = logger;
        var config = options.Value;
        var openAiClient = new OpenAIClient(config.ApiKey);
        _chatClient = openAiClient.GetChatClient("gpt-4o-mini");
    }

    /// <inheritdoc />
    public async Task<List<RecommendationResult>> GetRecommendationsAsync(
        Guid userId,
        List<BookLoan> history,
        List<Book> catalog,
        CancellationToken ct)
    {
        // For members with < 3 completed loans, return popular books (fallback)
        if (history.Count(l => l.ReturnedAt is not null) < MinHistoryForAi)
        {
            _logger.LogInformation("User {UserId} has fewer than {Min} completed loans. Using popular books fallback.", userId, MinHistoryForAi);
            return GetPopularBooksFallback(catalog);
        }

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(TimeoutSeconds));

            var recommendations = await GenerateAiRecommendationsAsync(history, catalog, timeoutCts.Token);
            return recommendations;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            _logger.LogWarning("OpenAI recommendation request timed out after {Timeout}s. Using popular books fallback.", TimeoutSeconds);
            return GetPopularBooksFallback(catalog);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate AI recommendations for user {UserId}. Using popular books fallback.", userId);
            return GetPopularBooksFallback(catalog);
        }
    }

    private async Task<List<RecommendationResult>> GenerateAiRecommendationsAsync(
        List<BookLoan> history,
        List<Book> catalog,
        CancellationToken ct)
    {
        var prompt = BuildPrompt(history, catalog);

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(
                "You are a librarian AI assistant. Based on a member's reading history, " +
                "recommend books from the available catalog. Respond ONLY with a valid JSON array. " +
                "Each element must have: bookId (GUID string), title, author, category, justification (max 200 chars). " +
                "Recommend 1-10 books. Do not recommend books the member already read."),
            new UserChatMessage(prompt)
        };

        var options = new ChatCompletionOptions
        {
            Temperature = 0.7f,
            MaxOutputTokenCount = 2000
        };

        ClientResult<ChatCompletion> response = await _chatClient.CompleteChatAsync(messages, options, ct);

        var content = response.Value.Content[0].Text;
        return ParseRecommendations(content, catalog);
    }

    private static string BuildPrompt(List<BookLoan> history, List<Book> catalog)
    {
        // Build reading history summary
        var completedLoans = history
            .Where(l => l.ReturnedAt is not null && l.Book is not null)
            .OrderByDescending(l => l.ReturnedAt)
            .Take(20)
            .ToList();

        var historyText = string.Join("\n", completedLoans.Select(l =>
            $"- \"{l.Book.Title}\" by {l.Book.Author} (Category: {l.Book.Category})"));

        // Build available catalog (limit to avoid token overflow)
        var availableBooks = catalog.Take(100).ToList();
        var catalogText = string.Join("\n", availableBooks.Select(b =>
            $"- ID: {b.Id} | \"{b.Title}\" by {b.Author} (Category: {b.Category}, Rating: {b.AverageRating:F1})"));

        return $"""
            ## Member's Reading History (most recent first):
            {historyText}

            ## Available Books in Catalog:
            {catalogText}

            Based on the reading history, recommend books from the catalog that this member would enjoy.
            Return a JSON array with 1-10 recommendations.
            """;
    }

    private List<RecommendationResult> ParseRecommendations(string content, List<Book> catalog)
    {
        try
        {
            // Extract JSON array from the response (handle markdown code blocks)
            var json = content.Trim();
            if (json.StartsWith("```"))
            {
                var firstNewline = json.IndexOf('\n');
                var lastFence = json.LastIndexOf("```");
                if (firstNewline > 0 && lastFence > firstNewline)
                {
                    json = json[(firstNewline + 1)..lastFence].Trim();
                }
            }

            var items = JsonSerializer.Deserialize<List<AiRecommendationDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items is null || items.Count == 0)
            {
                _logger.LogWarning("AI returned empty or unparseable recommendations.");
                return [];
            }

            // Map to domain results, validate against catalog
            var catalogLookup = catalog.ToDictionary(b => b.Id);
            var results = new List<RecommendationResult>();

            foreach (var item in items.Take(MaxRecommendations))
            {
                if (Guid.TryParse(item.BookId, out var bookId) && catalogLookup.TryGetValue(bookId, out var book))
                {
                    var justification = item.Justification?.Length > 200
                        ? item.Justification[..200]
                        : item.Justification ?? string.Empty;

                    results.Add(new RecommendationResult(
                        book.Id,
                        book.Title,
                        book.Author,
                        book.Category,
                        justification));
                }
            }

            return results;
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to parse AI recommendation response.");
            return [];
        }
    }

    /// <summary>
    /// Fallback: returns top borrowed books from the catalog, ordered by TotalRatings (proxy for popularity).
    /// Groups by category to provide variety.
    /// </summary>
    private static List<RecommendationResult> GetPopularBooksFallback(List<Book> catalog)
    {
        return catalog
            .OrderByDescending(b => b.TotalRatings)
            .ThenByDescending(b => b.AverageRating)
            .Take(MaxRecommendations)
            .Select(b => new RecommendationResult(
                b.Id,
                b.Title,
                b.Author,
                b.Category,
                "Popular book in the library catalog"))
            .ToList();
    }

    /// <summary>
    /// DTO for deserializing the AI response JSON.
    /// </summary>
    private sealed class AiRecommendationDto
    {
        public string BookId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Justification { get; set; } = string.Empty;
    }
}
