namespace MiniLibrary.Domain.ValueObjects;

/// <summary>
/// Value object representing a valid ISBN-13.
/// Validates 13-digit format and checksum (alternating weights 1 and 3, sum mod 10 = 0).
/// </summary>
public sealed record Isbn
{
    public string Value { get; }

    private Isbn(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates an Isbn value object after validating format and checksum.
    /// </summary>
    /// <param name="isbn">The ISBN-13 string (13 digits).</param>
    /// <returns>A valid Isbn instance.</returns>
    /// <exception cref="ArgumentException">Thrown when the ISBN is null, not 13 digits, or has an invalid checksum.</exception>
    public static Isbn Create(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN cannot be null or empty.", nameof(isbn));

        // Remove any hyphens or spaces for validation
        var cleaned = isbn.Replace("-", "").Replace(" ", "");

        if (cleaned.Length != 13)
            throw new ArgumentException("ISBN must be exactly 13 digits.", nameof(isbn));

        if (!cleaned.All(char.IsDigit))
            throw new ArgumentException("ISBN must contain only digits.", nameof(isbn));

        if (!IsValidChecksum(cleaned))
            throw new ArgumentException("ISBN has an invalid checksum.", nameof(isbn));

        return new Isbn(cleaned);
    }

    /// <summary>
    /// Validates ISBN-13 checksum using alternating weights of 1 and 3.
    /// The weighted sum of all 13 digits mod 10 must equal 0.
    /// </summary>
    private static bool IsValidChecksum(string isbn)
    {
        var sum = 0;
        for (var i = 0; i < 13; i++)
        {
            var digit = isbn[i] - '0';
            var weight = i % 2 == 0 ? 1 : 3;
            sum += digit * weight;
        }

        return sum % 10 == 0;
    }

    public override string ToString() => Value;
}
