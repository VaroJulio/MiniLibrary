using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Events;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Books.EventHandlers;

/// <summary>
/// Handles BookUpdatedEvent by regenerating the OpenAI embedding for the updated book.
/// If embedding generation fails, the book update completes without updating the embedding
/// and the failure is logged for future retry (Req 4.8).
/// </summary>
public sealed class GenerateEmbeddingOnBookUpdatedHandler : INotificationHandler<BookUpdatedEvent>
{
    private readonly IBookRepository _bookRepository;
    private readonly IEmbeddingService _embeddingService;
    private readonly IBookEmbeddingRepository _bookEmbeddingRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GenerateEmbeddingOnBookUpdatedHandler> _logger;

    public GenerateEmbeddingOnBookUpdatedHandler(
        IBookRepository bookRepository,
        IEmbeddingService embeddingService,
        IBookEmbeddingRepository bookEmbeddingRepository,
        IUnitOfWork unitOfWork,
        ILogger<GenerateEmbeddingOnBookUpdatedHandler> logger)
    {
        _bookRepository = bookRepository;
        _embeddingService = embeddingService;
        _bookEmbeddingRepository = bookEmbeddingRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BookUpdatedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetByIdAsync(notification.BookId, cancellationToken);
            if (book is null)
            {
                _logger.LogWarning("BookUpdatedEvent received for non-existent book {BookId}.", notification.BookId);
                return;
            }

            var textToEmbed = BuildEmbeddingText(book);
            var vector = await _embeddingService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

            if (vector is null)
            {
                _logger.LogWarning("Embedding regeneration failed for book {BookId}. Existing embedding retained.", notification.BookId);
                return;
            }

            var serializedVector = SerializeVector(vector);

            // Update existing embedding or create new one
            var existing = await _bookEmbeddingRepository.GetByBookIdAsync(notification.BookId, cancellationToken);
            if (existing is not null)
            {
                existing.Update(serializedVector);
                await _bookEmbeddingRepository.UpdateAsync(existing, cancellationToken);
            }
            else
            {
                var embedding = BookEmbedding.Create(notification.BookId, serializedVector);
                await _bookEmbeddingRepository.AddAsync(embedding, cancellationToken);
            }

            _logger.LogInformation("Embedding regenerated and stored for book {BookId}.", notification.BookId);

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            // Embedding failure must not prevent the book update from completing (Req 4.8)
            _logger.LogError(ex, "Unexpected error regenerating embedding for book {BookId}. Update completed without embedding change.", notification.BookId);
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
