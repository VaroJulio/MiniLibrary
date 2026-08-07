using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Commands.DeleteRating;

/// <summary>
/// Handles DeleteRatingCommand:
/// - Verifies the rating exists and belongs to the user
/// - Deletes the rating
/// - Recalculates book's AverageRating and TotalRatings
/// </summary>
public sealed class DeleteRatingCommandHandler : IRequestHandler<DeleteRatingCommand, Unit>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IBookRepository _bookRepository;

    public DeleteRatingCommandHandler(
        IRatingRepository ratingRepository,
        IBookRepository bookRepository)
    {
        _ratingRepository = ratingRepository;
        _bookRepository = bookRepository;
    }

    public async Task<Unit> Handle(DeleteRatingCommand request, CancellationToken cancellationToken)
    {
        var rating = await _ratingRepository.GetByUserAndBookAsync(
            request.UserId, request.BookId, cancellationToken);

        if (rating is null)
        {
            throw new NotFoundException("Rating", $"user={request.UserId}, book={request.BookId}");
        }

        await _ratingRepository.DeleteAsync(rating, cancellationToken);

        // Recalculate book's average rating (Req 16.8)
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is not null)
        {
            var (average, count) = await _ratingRepository.CalculateBookAverageAsync(
                request.BookId, cancellationToken);
            book.UpdateRatingStats(average, count);
            await _bookRepository.UpdateAsync(book, cancellationToken);
        }

        return Unit.Value;
    }
}
