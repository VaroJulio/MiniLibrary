using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Domain.ValueObjects;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for domain value objects and entities.
/// **Validates: Requirements 1.5, 11.4, 12.1, 12.3**
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

    // ── RelevanceScore Properties ────────────────────────────────────────────────

    /// <summary>
    /// **Validates: Requirements 12.1**
    /// RelevanceScore.Create() rejects values outside [0.0, 1.0].
    /// </summary>
    [Property(MaxTest = 100)]
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
}
