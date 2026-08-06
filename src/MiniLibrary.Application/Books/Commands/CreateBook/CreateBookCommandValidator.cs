using FluentValidation;

namespace MiniLibrary.Application.Books.Commands.CreateBook;

/// <summary>
/// Validates CreateBookCommand fields according to business rules.
/// </summary>
public class CreateBookCommandValidator : AbstractValidator<CreateBookCommand>
{
    public CreateBookCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(255).WithMessage("Title must not exceed 255 characters.");

        RuleFor(x => x.Author)
            .NotEmpty().WithMessage("Author is required.")
            .MaximumLength(200).WithMessage("Author must not exceed 200 characters.");

        RuleFor(x => x.Isbn)
            .NotEmpty().WithMessage("ISBN is required.")
            .Must(BeValidIsbn13).WithMessage("ISBN must be a valid 13-digit ISBN-13 format.");

        RuleFor(x => x.PublishedYear)
            .InclusiveBetween(1450, DateTime.UtcNow.Year)
            .WithMessage($"Published year must be between 1450 and {DateTime.UtcNow.Year}.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Category)
            .MaximumLength(100).WithMessage("Category must not exceed 100 characters.");
    }

    private static bool BeValidIsbn13(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            return false;

        var cleaned = isbn.Replace("-", "").Replace(" ", "");

        if (cleaned.Length != 13 || !cleaned.All(char.IsDigit))
            return false;

        // Validate ISBN-13 checksum
        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            var digit = cleaned[i] - '0';
            var weight = i % 2 == 0 ? 1 : 3;
            sum += digit * weight;
        }

        return sum % 10 == 0;
    }
}
