using MediatR;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Ratings.Queries.CanRateBook;

/// <summary>
/// Determines if the user has an unrated completed loan for a book.
/// Returns canRate=true with the loanId if the most recent completed loan has no associated rating.
/// </summary>
public sealed class CanRateBookQueryHandler : IRequestHandler<CanRateBookQuery, CanRateBookResponse>
{
    private readonly ILoanRepository _loanRepository;
    private readonly IRatingRepository _ratingRepository;

    public CanRateBookQueryHandler(ILoanRepository loanRepository, IRatingRepository ratingRepository)
    {
        _loanRepository = loanRepository;
        _ratingRepository = ratingRepository;
    }

    public async Task<CanRateBookResponse> Handle(CanRateBookQuery request, CancellationToken cancellationToken)
    {
        // Get the most recent completed loan
        var mostRecentLoan = await _loanRepository.GetMostRecentCompletedLoanAsync(
            request.BookId, request.UserId, cancellationToken);

        if (mostRecentLoan is null)
        {
            return new CanRateBookResponse(false, null);
        }

        // Check if a rating already exists for this loan
        var existingRating = await _ratingRepository.GetByLoanIdAsync(mostRecentLoan.Id, cancellationToken);

        if (existingRating is not null)
        {
            // Already rated for this loan cycle
            return new CanRateBookResponse(false, null);
        }

        return new CanRateBookResponse(true, mostRecentLoan.Id);
    }
}
