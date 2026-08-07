using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Events;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.EventHandlers;

/// <summary>
/// Handles BookCreatedEvent by generating an OpenAI embedding for the new book.
/// If embedding generation fails, the book operation completes without an embedding
/// and the failure is logged for future retry (Req 4.8).
/// </summary>
public sealed class GenerateEmbeddingOnBookCreatedHandler : INotificationHandler<BookCreatedEvent>
{
    private readonly IBookRepository _bookRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IBookEmbeddingRepository _bookEmbeddingRepository;
    private readonly ILogger<GenerateEmbeddingOnBookCreatedHandler> _logger;

    public GenerateEmbeddingOnBookCreatedHandler(
        IBookRepository bookRepository,
        IEmbeddingService embeddingService,
        IBookEmbeddingRepository bookEmbeddingRepository,
        ILogger<GenerateEmbeddingOnBookCreatedHandler> logger)
    {
        _bookRepository = bookRepository;
        _embeddingService = embeddingService;
        _bookEmbeddingRepository = bookEmbeddingRepository;
        _logger = logger;
    }

    public async Task Handle(BookCreatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetByIdAsync(notification.BookId, cancellationToken);
            if (book is null)
            {
                _logger.LogWarning("BookCreatedEvent received for non-existent book {BookId}.", notification.BookId);
                return;
            }

            var textToEmbed = BuildEmbeddingText(book);
            var vector = await _embeddingService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

            if (vector is null)
            {
                _logger.LogWarning("Embedding generation failed for book {BookId}. Book saved without embedding.", notification.BookId);
                return;
            }

            var serializedVector = SerializeVector(vector);
            var embedding = BookEmbedding.Create(notification.BookId, serializedVector);
            await _bookEmbeddingRepository.AddAsync(embedding, cancellationToken);

            _logger.LogInformation("Embedding generated and stored for book {BookId}.", notification.BookId);
        }
        catch (Exception ex)
        {
            // Embedding failure must not prevent the book operation from completing (Req 4.8)
            _logger.LogError(ex, "Unexpected error generating embedding for book {BookId}. Book saved without embedding.", notification.BookId);
        }
    }

    private static string BuildEmbeddingText(Book book)
    {
        return $"{book.Title} {book.Author} {book.Description}";
    }

    private static byte[] SerializeVector(float[] vector)
    {
        var bytes = new byte[vector.Length * sizeof(float)];
        Buffer.BlockCopy(vector, 0, bytes, 0, bytes.Length);
        return bytes;
    }
}
