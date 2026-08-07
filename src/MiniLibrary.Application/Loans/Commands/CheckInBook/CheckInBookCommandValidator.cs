using FluentValidation;

namespace MiniLibrary.Application.Loans.Commands.CheckInBook;

/// <summary>
/// Validates CheckInBookCommand fields.
/// </summary>
public class CheckInBookCommandValidator : AbstractValidator<CheckInBookCommand>
{
    public CheckInBookCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("BookId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
