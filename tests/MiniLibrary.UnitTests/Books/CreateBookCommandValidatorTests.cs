using FluentValidation.TestHelper;
using MiniLibrary.Application.Books.Commands.CreateBook;

namespace MiniLibrary.UnitTests.Books;

public class CreateBookCommandValidatorTests
{
    private readonly CreateBookCommandValidator _validator;

    public CreateBookCommandValidatorTests()
    {
        _validator = new CreateBookCommandValidator();
    }

    private static CreateBookCommand ValidCommand() => new(
        "Clean Code",
        "Robert C. Martin",
        "9780132350884",
        2008,
        "A Handbook of Agile Software Craftsmanship",
        "Software Engineering");

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        var result = await _validator.TestValidateAsync(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyTitle_FailsValidation(string? title)
    {
        var command = ValidCommand() with { Title = title! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_TitleExceeds255Chars_FailsValidation()
    {
        var command = ValidCommand() with { Title = new string('A', 256) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public async Task Validate_TitleExactly255Chars_PassesValidation()
    {
        var command = ValidCommand() with { Title = new string('A', 255) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task Validate_EmptyAuthor_FailsValidation(string? author)
    {
        var command = ValidCommand() with { Author = author! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public async Task Validate_AuthorExceeds200Chars_FailsValidation()
    {
        var command = ValidCommand() with { Author = new string('A', 201) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public async Task Validate_AuthorExactly200Chars_PassesValidation()
    {
        var command = ValidCommand() with { Author = new string('A', 200) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Author);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("123456789012")]      // 12 digits
    [InlineData("12345678901234")]    // 14 digits
    [InlineData("978013468599X")]     // Contains non-digit
    [InlineData("9780134685990")]     // Invalid checksum
    public async Task Validate_InvalidIsbn_FailsValidation(string? isbn)
    {
        var command = ValidCommand() with { Isbn = isbn! };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Isbn);
    }

    [Theory]
    [InlineData("9780132350884")]     // Valid ISBN-13
    [InlineData("9780134685991")]     // Valid ISBN-13
    public async Task Validate_ValidIsbn_PassesValidation(string isbn)
    {
        var command = ValidCommand() with { Isbn = isbn };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Isbn);
    }

    [Theory]
    [InlineData(1449)]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task Validate_YearBefore1450_FailsValidation(int year)
    {
        var command = ValidCommand() with { PublishedYear = year };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public async Task Validate_YearAfterCurrent_FailsValidation()
    {
        var command = ValidCommand() with { PublishedYear = DateTime.UtcNow.Year + 1 };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Theory]
    [InlineData(1450)]
    [InlineData(2000)]
    public async Task Validate_YearInValidRange_PassesValidation(int year)
    {
        var command = ValidCommand() with { PublishedYear = year };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public async Task Validate_CurrentYear_PassesValidation()
    {
        var command = ValidCommand() with { PublishedYear = DateTime.UtcNow.Year };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public async Task Validate_DescriptionExceeds2000Chars_FailsValidation()
    {
        var command = ValidCommand() with { Description = new string('A', 2001) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_DescriptionExactly2000Chars_PassesValidation()
    {
        var command = ValidCommand() with { Description = new string('A', 2000) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_EmptyDescription_PassesValidation()
    {
        var command = ValidCommand() with { Description = "" };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public async Task Validate_CategoryExceeds100Chars_FailsValidation()
    {
        var command = ValidCommand() with { Category = new string('A', 101) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public async Task Validate_CategoryExactly100Chars_PassesValidation()
    {
        var command = ValidCommand() with { Category = new string('A', 100) };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public async Task Validate_EmptyCategory_PassesValidation()
    {
        var command = ValidCommand() with { Category = "" };
        var result = await _validator.TestValidateAsync(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }
}
