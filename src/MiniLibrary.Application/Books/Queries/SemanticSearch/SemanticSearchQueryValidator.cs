using FluentValidation;

namespace MiniLibrary.Application.Books.Queries.SemanticSearch;

/// <summary>
/// Validates the SemanticSearchQuery: rejects empty/whitespace-only queries.
/// Queries exceeding 500 characters are truncated silently in the handler (not rejected).
/// </summary>
public sealed class SemanticSearchQueryValidator : AbstractValidator<SemanticSearchQuery>
{
    public SemanticSearchQueryValidator()
    {
        RuleFor(x => x.Query)
            .NotEmpty()
            .WithMessage("Search query must not be empty or whitespace.");
    }
}
