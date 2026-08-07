using FluentValidation;

namespace MiniLibrary.Application.Loans.Commands.CheckOutBook;

/// <summary>
/// Validates CheckOutBookCommand fields.
/// </summary>
public class CheckOutBookCommandValidator : AbstractValidator<CheckOutBookCommand>
{
    public CheckOutBookCommandValidator()
    {
        RuleFor(x => x.BookId)
            .NotEmpty().WithMessage("BookId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");
    }
}
