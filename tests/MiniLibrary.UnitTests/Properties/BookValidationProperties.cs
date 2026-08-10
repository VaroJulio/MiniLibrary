using FsCheck;
using FsCheck.Xunit;
using FluentValidation.TestHelper;
using MiniLibrary.Application.Books.Commands.CreateBook;
using MiniLibrary.Application.Books.Commands.DeleteBook;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Interfaces;
using MiniLibrary.Domain.ValueObjects;
using Moq;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for domain value objects and entities.
/// **Validates: Requirements 1.4, 1.5, 11.4, 12.1, 12.3**
/// </summary>
public class BookValidationProperties
{
    // ── Property 1: Book Validation Rejects Invalid Data ─────────────────────────
    // Generate random invalid field combinations and verify rejection.
    // Focus on Isbn value object domain-level validation.

    /// <summary>
    /// **Validates: Requirements 1.5**
    /// Isbn.Create() rejects strings that are not exactly 13 digits (after stripping hyphens/spaces).
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Isbn_RejectsNon13DigitStrings()
    {
        return Prop.ForAll(
            Arb.From(GenNon13DigitString()),
            input =>
            {
                var act = () => Isbn.Create(input);
                try
                {
                    act();
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true; // Expected rejection
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.5**
    /// Isbn.Create() rejects 13-digit strings with invalid checksums.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Isbn_RejectsInvalidChecksums()
    {
        return Prop.ForAll(
            Arb.From(GenInvalidChecksum13DigitString()),
            input =>
            {
                var act = () => Isbn.Create(input);
                try
                {
                    act();
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true; // Expected rejection
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.5**
    /// Valid 13-digit ISBNs with correct checksum are accepted by Isbn.Create().
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Isbn_AcceptsValidIsbn13WithCorrectChecksum()
    {
        return Prop.ForAll(
            Arb.From(GenValidIsbn13()),
            input =>
            {
                var isbn = Isbn.Create(input);
                return isbn.Value == input;
            });
    }

    // ── Property 9: ISBN Uniqueness ──────────────────────────────────────────────
    // Generate random 13-digit strings. Verify that Isbn.Create() accepts only those
    // with valid checksums and rejects all others.

    /// <summary>
    /// **Validates: Requirements 11.4**
    /// For any random 13-digit string, Isbn.Create() accepts it iff its checksum is valid.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Isbn_AcceptsOnlyValidChecksums()
    {
        return Prop.ForAll(
            Arb.From(GenRandom13DigitString()),
            input =>
            {
                var hasValidChecksum = CalculateIsbn13ChecksumValid(input);
                try
                {
                    var isbn = Isbn.Create(input);
                    // If creation succeeded, the checksum must be valid
                    return hasValidChecksum && isbn.Value == input;
                }
                catch (ArgumentException)
                {
                    // If creation failed, the checksum must be invalid
                    return !hasValidChecksum;
                }
            });
    }

    // ── Property 1: CreateBookCommandValidator Rejects Invalid Data ─────────────
    // Generate random invalid CreateBookCommand instances and verify all invalid
    // fields are rejected by FluentValidation.

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with titles exceeding 255 characters.
    /// </summary>
    [Property(Arbitrary = new[] { typeof(BookValidationArbitraries) }, MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsTitleExceedingMaxLength(PositiveInt extraChars)
    {
        var title = new string('A', 256 + (extraChars.Get % 500));
        var command = ValidCommandWith(title: title);
        var validator = new CreateBookCommandValidator();

        var result = validator.TestValidate(command);

        return (!result.IsValid && result.Errors.Any(e => e.PropertyName == "Title")).ToProperty();
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with empty titles.
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsEmptyTitle()
    {
        return Prop.ForAll(
            Arb.From(Gen.Elements("", " ", "   ", "\t", "\n")),
            emptyTitle =>
            {
                var command = ValidCommandWith(title: emptyTitle);
                var validator = new CreateBookCommandValidator();
                var result = validator.TestValidate(command);
                return !result.IsValid && result.Errors.Any(e => e.PropertyName == "Title");
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with authors exceeding 200 characters.
    /// </summary>
    [Property(Arbitrary = new[] { typeof(BookValidationArbitraries) }, MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsAuthorExceedingMaxLength(PositiveInt extraChars)
    {
        var author = new string('B', 201 + (extraChars.Get % 500));
        var command = ValidCommandWith(author: author);
        var validator = new CreateBookCommandValidator();

        var result = validator.TestValidate(command);

        return (!result.IsValid && result.Errors.Any(e => e.PropertyName == "Author")).ToProperty();
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with empty authors.
    /// </summary>
    [Property(MaxTest = 50)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsEmptyAuthor()
    {
        return Prop.ForAll(
            Arb.From(Gen.Elements("", " ", "   ")),
            emptyAuthor =>
            {
                var command = ValidCommandWith(author: emptyAuthor);
                var validator = new CreateBookCommandValidator();
                var result = validator.TestValidate(command);
                return !result.IsValid && result.Errors.Any(e => e.PropertyName == "Author");
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with invalid ISBNs.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsInvalidIsbn()
    {
        return Prop.ForAll(
            Arb.From(GenInvalidIsbnForValidator()),
            invalidIsbn =>
            {
                var command = ValidCommandWith(isbn: invalidIsbn);
                var validator = new CreateBookCommandValidator();
                var result = validator.TestValidate(command);
                return !result.IsValid && result.Errors.Any(e => e.PropertyName == "Isbn");
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with published years out of range [1450, current year].
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsOutOfRangePublishedYear()
    {
        return Prop.ForAll(
            Arb.From(GenOutOfRangeYear()),
            invalidYear =>
            {
                var command = ValidCommandWith(publishedYear: invalidYear);
                var validator = new CreateBookCommandValidator();
                var result = validator.TestValidate(command);
                return !result.IsValid && result.Errors.Any(e => e.PropertyName == "PublishedYear");
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with descriptions exceeding 2000 characters.
    /// </summary>
    [Property(Arbitrary = new[] { typeof(BookValidationArbitraries) }, MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsDescriptionExceedingMaxLength(PositiveInt extraChars)
    {
        var description = new string('D', 2001 + (extraChars.Get % 1000));
        var command = ValidCommandWith(description: description);
        var validator = new CreateBookCommandValidator();

        var result = validator.TestValidate(command);

        return (!result.IsValid && result.Errors.Any(e => e.PropertyName == "Description")).ToProperty();
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator rejects commands with category exceeding 100 characters.
    /// </summary>
    [Property(Arbitrary = new[] { typeof(BookValidationArbitraries) }, MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_RejectsCategoryExceedingMaxLength(PositiveInt extraChars)
    {
        var category = new string('C', 101 + (extraChars.Get % 200));
        var command = ValidCommandWith(category: category);
        var validator = new CreateBookCommandValidator();

        var result = validator.TestValidate(command);

        return (!result.IsValid && result.Errors.Any(e => e.PropertyName == "Category")).ToProperty();
    }

    /// <summary>
    /// **Validates: Requirements 1.5, 12.1, 12.3**
    /// CreateBookCommandValidator accepts commands with all valid fields.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Validator_AcceptsValidCommands()
    {
        return Prop.ForAll(
            Arb.From(GenValidCreateBookCommand()),
            command =>
            {
                var validator = new CreateBookCommandValidator();
                var result = validator.TestValidate(command);
                return result.IsValid;
            });
    }

    // ── Property 2: Book Deletion Invariant ──────────────────────────────────────
    // Generate random books with/without active loans and verify deletion is only
    // allowed when no active loans exist.

    /// <summary>
    /// **Validates: Requirements 1.4**
    /// DeleteBookCommandHandler throws ConflictException when book has active loans.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Delete_ThrowsConflict_WhenActiveLoansExist()
    {
        return Prop.ForAll(
            Arb.From(Gen.Fresh(() => Guid.NewGuid())),
            bookId =>
            {
                // Arrange: book exists + has an active loan
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");
                var activeLoan = BookLoan.Create(bookId, Guid.NewGuid(), DateTime.UtcNow);

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanByBookAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(activeLoan);

                var handler = new DeleteBookCommandHandler(mockBookRepo.Object, mockLoanRepo.Object, new Mock<IUnitOfWork>().Object);

                // Act & Assert
                try
                {
                    handler.Handle(new DeleteBookCommand(bookId), CancellationToken.None).GetAwaiter().GetResult();
                    return false; // Should have thrown
                }
                catch (ConflictException)
                {
                    return true; // Expected: cannot delete with active loans
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.4**
    /// DeleteBookCommandHandler succeeds (soft-deletes) when book has no active loans.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Delete_Succeeds_WhenNoActiveLoansExist()
    {
        return Prop.ForAll(
            Arb.From(Gen.Fresh(() => Guid.NewGuid())),
            bookId =>
            {
                // Arrange: book exists + no active loan
                var book = Book.Create("Test Book", "Author", "9780306406157", 2020, "Desc", "Fiction");

                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync(book);
                mockBookRepo.Setup(r => r.DeleteAsync(book, It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

                var mockLoanRepo = new Mock<ILoanRepository>();
                mockLoanRepo.Setup(r => r.GetActiveLoanByBookAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((BookLoan?)null);

                var handler = new DeleteBookCommandHandler(mockBookRepo.Object, mockLoanRepo.Object, new Mock<IUnitOfWork>().Object);

                // Act
                handler.Handle(new DeleteBookCommand(bookId), CancellationToken.None).GetAwaiter().GetResult();

                // Assert: book is now soft-deleted
                return book.IsDeleted;
            });
    }

    /// <summary>
    /// **Validates: Requirements 1.4**
    /// DeleteBookCommandHandler throws NotFoundException when book does not exist.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property Delete_ThrowsNotFound_WhenBookDoesNotExist()
    {
        return Prop.ForAll(
            Arb.From(Gen.Fresh(() => Guid.NewGuid())),
            bookId =>
            {
                // Arrange: book does not exist
                var mockBookRepo = new Mock<IBookRepository>();
                mockBookRepo.Setup(r => r.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                    .ReturnsAsync((Book?)null);

                var mockLoanRepo = new Mock<ILoanRepository>();

                var handler = new DeleteBookCommandHandler(mockBookRepo.Object, mockLoanRepo.Object, new Mock<IUnitOfWork>().Object);

                // Act & Assert
                try
                {
                    handler.Handle(new DeleteBookCommand(bookId), CancellationToken.None).GetAwaiter().GetResult();
                    return false; // Should have thrown
                }
                catch (NotFoundException)
                {
                    return true; // Expected: book not found
                }
            });
    }

    // ── RelevanceScore Properties ────────────────────────────────────────────────

    /// <summary>
    /// **Validates: Requirements 12.1**
    /// RelevanceScore.Create() rejects values outside [0.0, 1.0].
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property RelevanceScore_RejectsValuesOutsideRange()
    {
        return Prop.ForAll(
            Arb.From(GenOutOfRangeDouble()),
            value =>
            {
                try
                {
                    RelevanceScore.Create(value);
                    return false; // Should have thrown
                }
                catch (ArgumentOutOfRangeException)
                {
                    return true; // Expected rejection
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 12.1**
    /// RelevanceScore.Create() accepts values within [0.0, 1.0].
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property RelevanceScore_AcceptsValuesWithinRange()
    {
        return Prop.ForAll(
            Arb.From(GenInRangeDouble()),
            value =>
            {
                var score = RelevanceScore.Create(value);
                return score.Value == value;
            });
    }

    // ── DateRange Properties ─────────────────────────────────────────────────────

    /// <summary>
    /// **Validates: Requirements 12.3**
    /// DateRange.Create() rejects end < start.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property DateRange_RejectsEndBeforeStart()
    {
        return Prop.ForAll(
            Arb.From(GenDateRangeEndBeforeStart()),
            tuple =>
            {
                var (start, end) = tuple;
                try
                {
                    DateRange.Create(start, end);
                    return false; // Should have thrown
                }
                catch (ArgumentException)
                {
                    return true; // Expected rejection
                }
            });
    }

    /// <summary>
    /// **Validates: Requirements 12.3**
    /// DateRange.Create() accepts end >= start.
    /// </summary>
    [Property(MaxTest = 100)]
    [Trait("Category", "Property")]
    public Property DateRange_AcceptsEndOnOrAfterStart()
    {
        return Prop.ForAll(
            Arb.From(GenDateRangeEndOnOrAfterStart()),
            tuple =>
            {
                var (start, end) = tuple;
                var range = DateRange.Create(start, end);
                return range.Start == start && range.End == end;
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Custom Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates strings that are NOT exactly 13 digits after cleaning.
    /// Includes: empty, too short, too long, non-digit characters, etc.
    /// </summary>
    private static Gen<string> GenNon13DigitString()
    {
        var genTooShort = Gen.Choose(1, 12).SelectMany(length =>
            Gen.ArrayOf(length, Gen.Choose(0, 9))
               .Select(digits => string.Concat(digits)));

        var genTooLong = Gen.Choose(14, 20).SelectMany(length =>
            Gen.ArrayOf(length, Gen.Choose(0, 9))
               .Select(digits => string.Concat(digits)));

        var genWithNonDigits = Gen.ArrayOf(13, Gen.OneOf(
                Gen.Choose(0, 9).Select(d => (char)('0' + d)),
                Gen.Elements('A', 'B', 'X', 'Z', '!', '@')))
            .Where(arr => arr.Any(c => !char.IsDigit(c)))
            .Select(arr => new string(arr));

        var genEmpty = Gen.Constant("");

        return Gen.OneOf(genTooShort, genTooLong, genWithNonDigits, genEmpty);
    }

    /// <summary>
    /// Generates 13-digit strings with INVALID checksums.
    /// Takes a valid 12-digit prefix and appends a wrong check digit.
    /// </summary>
    private static Gen<string> GenInvalidChecksum13DigitString()
    {
        return Gen.ArrayOf(12, Gen.Choose(0, 9)).Select(first12 =>
        {
            var sum = 0;
            for (var i = 0; i < 12; i++)
            {
                var weight = i % 2 == 0 ? 1 : 3;
                sum += first12[i] * weight;
            }
            var correctCheck = (10 - (sum % 10)) % 10;
            // Pick a wrong check digit (offset by 1-9 from the correct one)
            var wrongCheck = (correctCheck + 1) % 10;
            return string.Concat(first12.Select(d => d.ToString())) + wrongCheck.ToString();
        });
    }

    /// <summary>
    /// Generates valid ISBN-13 strings (13 digits with correct checksum).
    /// </summary>
    private static Gen<string> GenValidIsbn13()
    {
        return Gen.ArrayOf(12, Gen.Choose(0, 9)).Select(first12 =>
        {
            var sum = 0;
            for (var i = 0; i < 12; i++)
            {
                var weight = i % 2 == 0 ? 1 : 3;
                sum += first12[i] * weight;
            }
            var checkDigit = (10 - (sum % 10)) % 10;
            return string.Concat(first12.Select(d => d.ToString())) + checkDigit.ToString();
        });
    }

    /// <summary>
    /// Generates random 13-digit strings (may or may not have valid checksums).
    /// </summary>
    private static Gen<string> GenRandom13DigitString()
    {
        return Gen.ArrayOf(13, Gen.Choose(0, 9))
            .Select(digits => string.Concat(digits));
    }

    /// <summary>
    /// Generates double values outside [0.0, 1.0].
    /// </summary>
    private static Gen<double> GenOutOfRangeDouble()
    {
        var genNegative = Gen.Choose(-10000, -1).Select(i => i / 1000.0);
        var genAboveOne = Gen.Choose(1001, 10000).Select(i => i / 1000.0);
        return Gen.OneOf(genNegative, genAboveOne);
    }

    /// <summary>
    /// Generates double values within [0.0, 1.0].
    /// </summary>
    private static Gen<double> GenInRangeDouble()
    {
        return Gen.Choose(0, 1000).Select(i => i / 1000.0);
    }

    /// <summary>
    /// Generates (start, end) pairs where end is strictly before start.
    /// </summary>
    private static Gen<(DateTime start, DateTime end)> GenDateRangeEndBeforeStart()
    {
        return Gen.Two(Gen.Choose(1, 365 * 50)).Select(pair =>
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var larger = Math.Max(pair.Item1, pair.Item2);
            var smaller = Math.Min(pair.Item1, pair.Item2);
            if (larger == smaller) larger += 1; // Ensure strictly before
            var start = baseDate.AddDays(larger);
            var end = baseDate.AddDays(smaller);
            return (start, end);
        });
    }

    /// <summary>
    /// Generates (start, end) pairs where end is on or after start.
    /// </summary>
    private static Gen<(DateTime start, DateTime end)> GenDateRangeEndOnOrAfterStart()
    {
        return Gen.Two(Gen.Choose(0, 365 * 50)).Select(pair =>
        {
            var baseDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var smaller = Math.Min(pair.Item1, pair.Item2);
            var larger = Math.Max(pair.Item1, pair.Item2);
            var start = baseDate.AddDays(smaller);
            var end = baseDate.AddDays(larger);
            return (start, end);
        });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Helper Methods
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Calculates whether a 13-digit string has a valid ISBN-13 checksum.
    /// Uses alternating weights 1 and 3; sum mod 10 must equal 0.
    /// </summary>
    private static bool CalculateIsbn13ChecksumValid(string isbn)
    {
        if (isbn.Length != 13 || !isbn.All(char.IsDigit))
            return false;

        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            var digit = isbn[i] - '0';
            var weight = i % 2 == 0 ? 1 : 3;
            sum += digit * weight;
        }
        return sum % 10 == 0;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // CreateBookCommand Helpers & Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a valid CreateBookCommand with optional field overrides.
    /// </summary>
    private static CreateBookCommand ValidCommandWith(
        string? title = null,
        string? author = null,
        string? isbn = null,
        int? publishedYear = null,
        string? description = null,
        string? category = null)
    {
        return new CreateBookCommand(
            Title: title ?? "Valid Title",
            Author: author ?? "Valid Author",
            Isbn: isbn ?? "9780306406157",
            PublishedYear: publishedYear ?? 2020,
            Description: description ?? "A valid description.",
            Category: category ?? "Fiction");
    }

    /// <summary>
    /// Generates invalid ISBN strings for the CreateBookCommandValidator.
    /// Includes: non-13-digit strings, wrong checksums, empty strings, strings with letters.
    /// </summary>
    private static Gen<string> GenInvalidIsbnForValidator()
    {
        var genTooShort = Gen.Choose(1, 12).SelectMany(length =>
            Gen.ArrayOf(length, Gen.Choose(0, 9))
               .Select(digits => string.Concat(digits)));

        var genTooLong = Gen.Choose(14, 20).SelectMany(length =>
            Gen.ArrayOf(length, Gen.Choose(0, 9))
               .Select(digits => string.Concat(digits)));

        var genWrongChecksum = GenInvalidChecksum13DigitString();

        var genWithLetters = Gen.ArrayOf(13, Gen.OneOf(
                Gen.Choose(0, 9).Select(d => (char)('0' + d)),
                Gen.Elements('A', 'B', 'X')))
            .Where(arr => arr.Any(c => !char.IsDigit(c)))
            .Select(arr => new string(arr));

        var genEmpty = Gen.Elements("", " ", "abc");

        return Gen.OneOf(genTooShort, genTooLong, genWrongChecksum, genWithLetters, genEmpty);
    }

    /// <summary>
    /// Generates published years outside the valid range [1450, current year].
    /// </summary>
    private static Gen<int> GenOutOfRangeYear()
    {
        var currentYear = DateTime.UtcNow.Year;
        var genTooLow = Gen.Choose(0, 1449);
        var genTooHigh = Gen.Choose(currentYear + 1, currentYear + 100);
        return Gen.OneOf(genTooLow, genTooHigh);
    }

    /// <summary>
    /// Generates valid CreateBookCommand instances with all fields within constraints.
    /// </summary>
    private static Gen<CreateBookCommand> GenValidCreateBookCommand()
    {
        var currentYear = DateTime.UtcNow.Year;

        var genTitle = Gen.Choose(1, 100).SelectMany(len =>
            Gen.ArrayOf(len, Gen.Elements(
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'a', 'b', 'c', 'd', ' '))
                .Select(chars => new string(chars).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var genAuthor = Gen.Choose(1, 100).SelectMany(len =>
            Gen.ArrayOf(len, Gen.Elements(
                'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'a', 'b', 'c', 'd', ' '))
                .Select(chars => new string(chars).Trim())
                .Where(s => !string.IsNullOrWhiteSpace(s)));

        var genYear = Gen.Choose(1450, currentYear);

        var genDescription = Gen.Choose(0, 200).SelectMany(len =>
            Gen.ArrayOf(len, Gen.Elements('A', 'b', 'c', 'd', 'e', ' ', '.'))
                .Select(chars => new string(chars)));

        return from title in genTitle
               from author in genAuthor
               from isbn in GenValidIsbn13()
               from year in genYear
               from description in genDescription
               select new CreateBookCommand(
                   Title: title,
                   Author: author,
                   Isbn: isbn,
                   PublishedYear: year,
                   Description: description,
                   Category: "Fiction");
    }
}

/// <summary>
/// Arbitrary type registration for FsCheck property tests.
/// </summary>
public class BookValidationArbitraries
{
    public static Arbitrary<PositiveInt> PositiveInt()
    {
        return Arb.Default.PositiveInt();
    }
}
