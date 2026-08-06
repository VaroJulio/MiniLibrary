namespace MiniLibrary.Domain.ValueObjects;

/// <summary>
/// Value object representing a relevance score for semantic search results,
/// constrained to the range [0.0, 1.0].
/// </summary>
public sealed record RelevanceScore : IComparable<RelevanceScore>
{
    public double Value { get; }

    private RelevanceScore(double value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a RelevanceScore value object after validating the value is within [0.0, 1.0].
    /// </summary>
    /// <param name="value">The relevance score value.</param>
    /// <returns>A valid RelevanceScore instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when value is outside [0.0, 1.0].</exception>
    public static RelevanceScore Create(double value)
    {
        if (value < 0.0 || value > 1.0)
            throw new ArgumentOutOfRangeException(nameof(value), value, "Relevance score must be between 0.0 and 1.0.");

        return new RelevanceScore(value);
    }

    /// <summary>
    /// Checks whether this score meets the minimum threshold for inclusion in search results.
    /// Default threshold is 0.3 as per requirements.
    /// </summary>
    public bool MeetsThreshold(double threshold = 0.3) => Value >= threshold;

    public int CompareTo(RelevanceScore? other)
    {
        if (other is null) return 1;
        return Value.CompareTo(other.Value);
    }

    public static bool operator >(RelevanceScore left, RelevanceScore right) => left.Value > right.Value;
    public static bool operator <(RelevanceScore left, RelevanceScore right) => left.Value < right.Value;
    public static bool operator >=(RelevanceScore left, RelevanceScore right) => left.Value >= right.Value;
    public static bool operator <=(RelevanceScore left, RelevanceScore right) => left.Value <= right.Value;

    public override string ToString() => Value.ToString("F4");
}
