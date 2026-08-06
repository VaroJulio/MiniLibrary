namespace MiniLibrary.Domain.Events;

/// <summary>
/// Raised when a book loan is returned.
/// Triggers badge evaluation and wishlist availability alerts.
/// </summary>
public record BookReturnedEvent(Guid BookId, Guid UserId, Guid LoanId);
