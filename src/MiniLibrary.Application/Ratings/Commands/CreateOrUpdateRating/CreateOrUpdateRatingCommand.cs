using MediatR;
using MiniLibrary.Application.Ratings.DTOs;

namespace MiniLibrary.Application.Ratings.Commands.CreateOrUpdateRating;

/// <summary>
/// Command to create or update a rating for a book.
/// If the user already has a rating for the book, it is updated; otherwise a new one is created.
/// </summary>
public sealed record CreateOrUpdateRatingCommand : IRequest<RatingResponse>
{
    public Guid BookId { get; init; }
    public Guid UserId { get; init; }
    public int Score { get; init; }
    public string ReviewText { get; init; } = string.Empty;
}
