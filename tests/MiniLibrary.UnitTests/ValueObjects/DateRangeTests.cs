using MiniLibrary.Domain.ValueObjects;

namespace MiniLibrary.UnitTests.ValueObjects;

public class DateRangeTests
{
    private static readonly DateTime _today = new DateTime(2024, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    // ── Happy path ──────────────────────────────────────────────────────────────

    [Fact]
    public void Create_ValidRange_ReturnsDatedRange()
    {
        var start = _today;
        var end = _today.AddDays(14);

        var range = DateRange.Create(start, end);

        range.Start.Should().Be(start);
        range.End.Should().Be(end);
    }

    [Fact]
    public void Create_SameDateStartAndEnd_Succeeds()
    {
        var date = _today;

        var act = () => DateRange.Create(date, date);

        act.Should().NotThrow();
    }

    [Fact]
    public void CreateLoanPeriod_Returns14DayRange()
    {
        var range = DateRange.CreateLoanPeriod(_today);

        range.Start.Should().Be(_today);
        range.End.Should().Be(_today.AddDays(14));
        range.TotalDays.Should().Be(14);
    }

    // ── Validation errors ────────────────────────────────────────────────────────

    [Fact]
    public void Create_EndBeforeStart_ThrowsArgumentException()
    {
        var start = _today;
        var end = _today.AddDays(-1);

        var act = () => DateRange.Create(start, end);

        act.Should().Throw<ArgumentException>()
           .WithParameterName("end");
    }

    // ── TotalDays ────────────────────────────────────────────────────────────────

    [Fact]
    public void TotalDays_ReturnsCorrectCount()
    {
        var range = DateRange.Create(_today, _today.AddDays(7));

        range.TotalDays.Should().Be(7);
    }

    // ── Contains ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Contains_DateWithinRange_ReturnsTrue()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.Contains(_today.AddDays(7)).Should().BeTrue();
    }

    [Fact]
    public void Contains_StartDate_ReturnsTrue()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.Contains(range.Start).Should().BeTrue();
    }

    [Fact]
    public void Contains_EndDate_ReturnsTrue()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.Contains(range.End).Should().BeTrue();
    }

    [Fact]
    public void Contains_DateBeforeRange_ReturnsFalse()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.Contains(_today.AddDays(-1)).Should().BeFalse();
    }

    [Fact]
    public void Contains_DateAfterRange_ReturnsFalse()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.Contains(_today.AddDays(15)).Should().BeFalse();
    }

    // ── IsOverdue ────────────────────────────────────────────────────────────────

    [Fact]
    public void IsOverdue_CurrentDateAfterEnd_ReturnsTrue()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.IsOverdue(_today.AddDays(15)).Should().BeTrue();
    }

    [Fact]
    public void IsOverdue_CurrentDateOnEnd_ReturnsFalse()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.IsOverdue(range.End).Should().BeFalse();
    }

    [Fact]
    public void IsOverdue_CurrentDateBeforeEnd_ReturnsFalse()
    {
        var range = DateRange.Create(_today, _today.AddDays(14));

        range.IsOverdue(_today.AddDays(7)).Should().BeFalse();
    }

    // ── Value semantics ─────────────────────────────────────────────────────────

    [Fact]
    public void TwoDateRanges_WithSameDates_AreEqual()
    {
        var a = DateRange.Create(_today, _today.AddDays(14));
        var b = DateRange.Create(_today, _today.AddDays(14));

        a.Should().Be(b);
    }

    [Fact]
    public void TwoDateRanges_WithDifferentDates_AreNotEqual()
    {
        var a = DateRange.Create(_today, _today.AddDays(14));
        var b = DateRange.Create(_today, _today.AddDays(7));

        a.Should().NotBe(b);
    }
}
