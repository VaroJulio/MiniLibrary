using MiniLibrary.Domain.ValueObjects;

namespace MiniLibrary.UnitTests.ValueObjects;

public class RelevanceScoreTests
{
    // ── Happy path ──────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void Create_ValidValue_ReturnsRelevanceScore(double value)
    {
        var score = RelevanceScore.Create(value);

        score.Value.Should().Be(value);
    }

    // ── Validation errors ────────────────────────────────────────────────────────

    [Theory]
    [InlineData(-0.001)]
    [InlineData(-1.0)]
    [InlineData(1.001)]
    [InlineData(2.0)]
    public void Create_OutOfRange_ThrowsArgumentOutOfRangeException(double value)
    {
        var act = () => RelevanceScore.Create(value);

        act.Should().Throw<ArgumentOutOfRangeException>()
           .WithParameterName("value");
    }

    // ── MeetsThreshold ───────────────────────────────────────────────────────────

    [Theory]
    [InlineData(0.3)]
    [InlineData(0.5)]
    [InlineData(1.0)]
    public void MeetsThreshold_AtOrAboveDefaultThreshold_ReturnsTrue(double value)
    {
        var score = RelevanceScore.Create(value);

        score.MeetsThreshold().Should().BeTrue();
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(0.1)]
    [InlineData(0.29)]
    public void MeetsThreshold_BelowDefaultThreshold_ReturnsFalse(double value)
    {
        var score = RelevanceScore.Create(value);

        score.MeetsThreshold().Should().BeFalse();
    }

    [Fact]
    public void MeetsThreshold_CustomThreshold_UsesProvidedValue()
    {
        var score = RelevanceScore.Create(0.6);

        score.MeetsThreshold(0.7).Should().BeFalse();
        score.MeetsThreshold(0.5).Should().BeTrue();
    }

    // ── Comparison operators ─────────────────────────────────────────────────────

    [Fact]
    public void GreaterThan_Higher_ReturnsTrue()
    {
        var high = RelevanceScore.Create(0.8);
        var low = RelevanceScore.Create(0.3);

        (high > low).Should().BeTrue();
    }

    [Fact]
    public void LessThan_Lower_ReturnsTrue()
    {
        var low = RelevanceScore.Create(0.3);
        var high = RelevanceScore.Create(0.8);

        (low < high).Should().BeTrue();
    }

    [Fact]
    public void CompareTo_OrdersCorrectly()
    {
        var a = RelevanceScore.Create(0.5);
        var b = RelevanceScore.Create(0.8);

        a.CompareTo(b).Should().BeNegative();
        b.CompareTo(a).Should().BePositive();
        a.CompareTo(RelevanceScore.Create(0.5)).Should().Be(0);
    }

    [Fact]
    public void CompareTo_Null_ReturnsPositive()
    {
        var score = RelevanceScore.Create(0.5);

        score.CompareTo(null).Should().BePositive();
    }

    // ── Value semantics ─────────────────────────────────────────────────────────

    [Fact]
    public void TwoScores_WithSameValue_AreEqual()
    {
        var a = RelevanceScore.Create(0.75);
        var b = RelevanceScore.Create(0.75);

        a.Should().Be(b);
    }

    [Fact]
    public void TwoScores_WithDifferentValues_AreNotEqual()
    {
        var a = RelevanceScore.Create(0.5);
        var b = RelevanceScore.Create(0.6);

        a.Should().NotBe(b);
    }

    // ── ToString ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ToString_Returns4DecimalPlaces()
    {
        var score = RelevanceScore.Create(0.75);

        score.ToString().Should().Be("0.7500");
    }
}
