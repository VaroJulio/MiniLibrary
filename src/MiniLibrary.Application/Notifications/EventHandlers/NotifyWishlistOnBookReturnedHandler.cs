using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Events;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Notifications.EventHandlers;

/// <summary>
/// Handles BookReturnedEvent: notifies all members who have the book in their wishlist
/// that it is now available.
/// </summary>
public sealed class NotifyWishlistOnBookReturnedHandler : INotificationHandler<BookReturnedEvent>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IBookRepository _bookRepository;
    private readonly INotificationService _notificationService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotifyWishlistOnBookReturnedHandler> _logger;

    public NotifyWishlistOnBookReturnedHandler(
        IWishlistRepository wishlistRepository,
        IBookRepository bookRepository,
        INotificationService notificationService,
        IUnitOfWork unitOfWork,
        ILogger<NotifyWishlistOnBookReturnedHandler> logger)
    {
        _wishlistRepository = wishlistRepository;
        _bookRepository = bookRepository;
        _notificationService = notificationService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(BookReturnedEvent notification, CancellationToken cancellationToken)
    {
        try
        {
            var book = await _bookRepository.GetByIdAsync(notification.BookId, cancellationToken);
            if (book is null) return;

            var watchers = await _wishlistRepository.GetBookWatchersAsync(notification.BookId, cancellationToken);

            foreach (var watcher in watchers)
            {
                // Don't notify the user who just returned it
                if (watcher.UserId == notification.UserId) continue;

                await _notificationService.SendInAppAsync(
                    watcher.UserId,
                    "Book Available!",
                    $"\"{book.Title}\" by {book.Author} is now available for checkout.",
                    NotificationType.BookAvailable,
                    cancellationToken);
            }

            if (watchers.Count > 0)
                _logger.LogInformation("Sent availability alerts to {Count} watchers for book {BookId}.", watchers.Count, notification.BookId);

            await _unitOfWork.CommitAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send wishlist availability alerts for book {BookId}.", notification.BookId);
        }
    }
}
