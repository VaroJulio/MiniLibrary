using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Application.Ratings.DTOs;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Commands.CreateOrUpdateRating;

/// <summary>
/// Handles CreateOrUpdateRatingCommand:
/// - Validates member has a completed loan for the book
/// - Enforces one rating per loan cycle (tied to specific loan)
/// - Creates a new rating or updates the existing one for that loan
/// - Recalculates book's AverageRating and TotalRatings
/// </summary>
public sealed class CreateOrUpdateRatingCommandHandler
    : IRequestHandler<CreateOrUpdateRatingCommand, RatingResponse>
{
    private readonly IRatingRepository _ratingRepository;
    private readonly IBookRepository _bookRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrUpdateRatingCommandHandler(
        IRatingRepository ratingRepository,
        IBookRepository bookRepository,
        ILoanRepository loanRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _ratingRepository = ratingRepository;
        _bookRepository = bookRepository;
        _loanRepository = loanRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
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

        // Get the most recent completed loan for this user+book
        var mostRecentLoan = await _loanRepository.GetMostRecentCompletedLoanAsync(
            request.BookId, request.UserId, cancellationToken);

        if (mostRecentLoan is null)
        {
            throw new ForbiddenException("You must have completed a loan for this book before rating it.");
        }

        // Check if a rating already exists for this specific loan cycle
        var existingForLoan = await _ratingRepository.GetByLoanIdAsync(mostRecentLoan.Id, cancellationToken);

        Rating rating;
        if (existingForLoan is not null)
        {
            // Update existing rating for this loan cycle
            existingForLoan.Update(request.Score, request.ReviewText);
            await _ratingRepository.UpdateAsync(existingForLoan, cancellationToken);
            rating = existingForLoan;
        }
        else
        {
            // Create new rating tied to this loan cycle
            rating = Rating.Create(request.BookId, request.UserId, request.Score, request.ReviewText, mostRecentLoan.Id);
            await _ratingRepository.AddAsync(rating, cancellationToken);
        }

        // Recalculate book's average rating
        var (average, count) = await _ratingRepository.CalculateBookAverageAsync(
            request.BookId, cancellationToken);
        book.UpdateRatingStats(average, count);
        await _bookRepository.UpdateAsync(book, cancellationToken);

        // Commit all changes atomically
        await _unitOfWork.CommitAsync(cancellationToken);

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
