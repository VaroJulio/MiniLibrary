namespace MiniLibrary.Domain.ValueObjects;

/// <summary>
/// Value object representing a date range (start to end) used for loan periods.
/// Validates that the end date is on or after the start date.
/// </summary>
public sealed record DateRange
{
    public DateTime Start { get; }
    public DateTime End { get; }

    private DateRange(DateTime start, DateTime end)
    {
        Start = start;
        End = end;
    }

    /// <summary>
    /// Creates a DateRange value object after validating that end >= start.
    /// </summary>
    /// <param name="start">The start date of the range.</param>
    /// <param name="end">The end date of the range.</param>
    /// <returns>A valid DateRange instance.</returns>
    /// <exception cref="ArgumentException">Thrown when end is before start.</exception>
    public static DateRange Create(DateTime start, DateTime end)
    {
        if (end < start)
            throw new ArgumentException("End date must be on or after the start date.", nameof(end));

        return new DateRange(start, end);
    }

    /// <summary>
    /// Creates a loan period starting from a given date with the standard 14-day duration.
    /// </summary>
    /// <param name="borrowedAt">The date the loan starts.</param>
    /// <returns>A DateRange representing a 14-day loan period.</returns>
    public static DateRange CreateLoanPeriod(DateTime borrowedAt)
    {
        return new DateRange(borrowedAt, borrowedAt.AddDays(14));
    }

    /// <summary>
    /// Gets the total number of days in the range.
    /// </summary>
    public int TotalDays => (End - Start).Days;

    /// <summary>
    /// Checks whether a given date falls within this range (inclusive).
    /// </summary>
    public bool Contains(DateTime date) => date >= Start && date <= End;

    /// <summary>
    /// Checks whether the end date has passed relative to the given date.
    /// </summary>
    public bool IsOverdue(DateTime currentDate) => currentDate > End;

    public override string ToString() => $"{Start:yyyy-MM-dd} to {End:yyyy-MM-dd}";
}
