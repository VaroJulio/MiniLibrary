using MediatR;

namespace MiniLibrary.Application.Ratings.Commands.DeleteRating;

/// <summary>
/// Command to delete the current user's rating for a book.
/// Recalculates the book's average after deletion.
/// </summary>
public sealed record DeleteRatingCommand(Guid BookId, Guid UserId) : IRequest<Unit>;
