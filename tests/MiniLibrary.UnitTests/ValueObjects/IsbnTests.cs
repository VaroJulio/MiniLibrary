using MiniLibrary.Domain.ValueObjects;

namespace MiniLibrary.UnitTests.ValueObjects;

public class IsbnTests
{
    // ── Happy path ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidIsbn13_ReturnsIsbn()
    {
        var isbn = Isbn.Create("9780306406157");

        isbn.Value.Should().Be("9780306406157");
    }

    [Fact]
    public void Create_ValidIsbn13WithHyphens_StripsAndReturnsIsbn()
    {
        // Hyphens are allowed in the raw input and should be stripped
        var isbn = Isbn.Create("978-0-306-40615-7");

        isbn.Value.Should().Be("9780306406157");
    }

    [Fact]
    public void Create_ValidIsbn_ToString_ReturnsBareDigits()
    {
        var isbn = Isbn.Create("9780306406157");

        isbn.ToString().Should().Be("9780306406157");
    }

    // ── Value semantics ─────────────────────────────────────────────────────────

    [Fact]
    public void TwoIsbns_WithSameValue_AreEqual()
    {
        var a = Isbn.Create("9780306406157");
        var b = Isbn.Create("9780306406157");

        a.Should().Be(b);
    }

    [Fact]
    public void TwoIsbns_WithDifferentValues_AreNotEqual()
    {
        var a = Isbn.Create("9780306406157");
        var b = Isbn.Create("9780140449136"); // valid ISBN-13

        a.Should().NotBe(b);
    }

    // ── Validation errors ────────────────────────────────────────────────────────

    [Fact]
    public void Create_NullIsbn_ThrowsArgumentException()
    {
        var act = () => Isbn.Create(null!);

        act.Should().Throw<ArgumentException>()
           .WithParameterName("isbn");
    }

    [Fact]
    public void Create_EmptyIsbn_ThrowsArgumentException()
    {
        var act = () => Isbn.Create("");

        act.Should().Throw<ArgumentException>()
           .WithParameterName("isbn");
    }

    [Theory]
    [InlineData("123456789012")]   // 12 digits
    [InlineData("12345678901234")] // 14 digits
    public void Create_WrongLength_ThrowsArgumentException(string isbn)
    {
        var act = () => Isbn.Create(isbn);

        act.Should().Throw<ArgumentException>()
           .WithParameterName("isbn");
    }

    [Fact]
    public void Create_NonDigitCharacters_ThrowsArgumentException()
    {
        var act = () => Isbn.Create("978030640615A");

        act.Should().Throw<ArgumentException>()
           .WithParameterName("isbn");
    }

    [Fact]
    public void Create_FailsChecksum_ThrowsArgumentException()
    {
        // Valid length and digits, but checksum digit changed to wrong value
        var act = () => Isbn.Create("9780306406158"); // last digit should be 7

        act.Should().Throw<ArgumentException>()
           .WithParameterName("isbn");
    }

    // ── Additional valid ISBNs ───────────────────────────────────────────────────

    [Theory]
    [InlineData("9780140449136")] // Dante's Inferno (Penguin)
    [InlineData("9780743273565")] // The Great Gatsby
    [InlineData("9780061965081")] // The Hunger Games
    public void Create_WellKnownValidIsbn13_Succeeds(string isbn)
    {
        var act = () => Isbn.Create(isbn);

        act.Should().NotThrow();
    }
}
