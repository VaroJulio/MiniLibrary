using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Commands.CreateOrUpdateRating;

/// <summary>
/// Handles CreateOrUpdateRatingCommand:
/// - Validates member has completed a loan for the book
/// - Creates or updates the rating
/// - Recalculates book's AverageRating and TotalRatings
/// </summary>
public sealed class CreateOrUpdateRatingCommandHandler
    : IRequestHandler<CreateOrUpdateRatingCommand, RatingResponse>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IUserRepository _userRepository;

    public CreateOrUpdateRatingCommandHandler(
        IRatingRepository ratingRepository,
        IBookRepository bookRepository,
        ILoanRepository loanRepository,
        IUserRepository userRepository)
    {
        _ratingRepository = ratingRepository;
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
        _userRepository = userRepository;
    }

    public async Task<RatingResponse> Handle(
        CreateOrUpdateRatingCommand request,
        CancellationToken cancellationToken)
    {
        // Verify book exists
        var book = await _bookRepository.GetByIdAsync(request.BookId, cancellationToken);
        if (book is null)
        {
            throw new NotFoundException("Book", request.BookId);
        }

        // Verify member has completed a loan for this book (Req 16.3)
        var hasCompletedLoan = await _loanRepository.HasCompletedLoanAsync(
            request.BookId, request.UserId, cancellationToken);
        if (!hasCompletedLoan)
        {
            throw new ForbiddenException("You must have completed a loan for this book before rating it.");
        }

        // Check if rating already exists
        var existing = await _ratingRepository.GetByUserAndBookAsync(
            request.UserId, request.BookId, cancellationToken);

        Rating rating;
        if (existing is not null)
        {
            // Update existing rating
            existing.Update(request.Score, request.ReviewText);
            await _ratingRepository.UpdateAsync(existing, cancellationToken);
            rating = existing;
        }
        else
        {
            // Create new rating
            rating = Rating.Create(request.BookId, request.UserId, request.Score, request.ReviewText);
            await _ratingRepository.AddAsync(rating, cancellationToken);
        }

        // Recalculate book's average rating (Req 16.4)
        var (average, count) = await _ratingRepository.CalculateBookAverageAsync(
            request.BookId, cancellationToken);
        book.UpdateRatingStats(average, count);
        await _bookRepository.UpdateAsync(book, cancellationToken);

        // Get user name for response
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        return new RatingResponse(
            rating.Id,
            rating.BookId,
            rating.UserId,
            user?.FullName ?? "Unknown",
            rating.Score,
            rating.ReviewText,
            rating.UsefulVotes,
            rating.CreatedAt,
            rating.UpdatedAt);
    }
}
