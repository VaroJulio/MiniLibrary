using FluentValidation;

namespace MiniLibrary.Application.Ratings.Commands.CreateOrUpdateRating;

/// <summary>
/// Validates CreateOrUpdateRatingCommand: score 1-5, review text max 1000 chars.
/// </summary>
public sealed class CreateOrUpdateRatingCommandValidator : AbstractValidator<CreateOrUpdateRatingCommand>
{
    public CreateOrUpdateRatingCommandValidator()
    {
        RuleFor(x => x.Score)
            .InclusiveBetween(1, 5)
            .WithMessage("Score must be between 1 and 5.");

        RuleFor(x => x.ReviewText)
            .MaximumLength(1000)
            .WithMessage("Review text must not exceed 1000 characters.");

        RuleFor(x => x.BookId)
            .NotEmpty()
            .WithMessage("Book ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");
    }
}
