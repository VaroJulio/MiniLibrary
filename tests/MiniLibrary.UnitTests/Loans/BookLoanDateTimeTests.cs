using MiniLibrary.Domain.Entities;

namespace MiniLibrary.UnitTests.Loans;

/// <summary>
/// Unit tests for BookLoan.IsOverdueAt() and DaysUntilDueAt() parameterized methods.
/// These tests use deterministic dates — no dependency on system clock.
/// </summary>
public class BookLoanDateTimeTests
{
    private static readonly Guid ValidBookId = Guid.NewGuid();
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly DateTime BorrowedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    // 14-day loan: DueDate = 2026-01-15 12:00:00 UTC
    private static readonly DateTime DueDate = BorrowedAt.AddDays(14);

    private static BookLoan CreateActiveLoan() =>
        BookLoan.Create(ValidBookId, ValidUserId, BorrowedAt, 14);

    private static BookLoan CreateReturnedLoan()
    {
        var loan = BookLoan.Create(ValidBookId, ValidUserId, BorrowedAt, 14);
        loan.Return(BorrowedAt.AddDays(7));
        return loan;
    }

    // ── IsOverdueAt ──────────────────────────────────────────────────────────

    [Fact]
    public void IsOverdueAt_BeforeDueDate_ReturnsFalse()
    {
        var loan = CreateActiveLoan();
        var threeDaysBefore = DueDate.AddDays(-3);

        loan.IsOverdueAt(threeDaysBefore).Should().BeFalse();
    }

    [Fact]
    public void IsOverdueAt_ExactlyAtDueDate_ReturnsFalse()
    {
        var loan = CreateActiveLoan();

        loan.IsOverdueAt(DueDate).Should().BeFalse();
    }

    [Fact]
    public void IsOverdueAt_OneSecondAfterDueDate_ReturnsTrue()
    {
        var loan = CreateActiveLoan();
        var oneSecondAfter = DueDate.AddSeconds(1);

        loan.IsOverdueAt(oneSecondAfter).Should().BeTrue();
    }

    [Fact]
    public void IsOverdueAt_OneDayAfterDueDate_ReturnsTrue()
    {
        var loan = CreateActiveLoan();
        var oneDayAfter = DueDate.AddDays(1);

        loan.IsOverdueAt(oneDayAfter).Should().BeTrue();
    }

    [Fact]
    public void IsOverdueAt_ReturnedLoan_AlwaysReturnsFalse()
    {
        var loan = CreateReturnedLoan();
        var wellAfterDue = DueDate.AddDays(30);

        loan.IsOverdueAt(wellAfterDue).Should().BeFalse();
    }

    [Fact]
    public void IsOverdueAt_BorrowedSameDay_ReturnsFalse()
    {
        var loan = CreateActiveLoan();

        loan.IsOverdueAt(BorrowedAt).Should().BeFalse();
    }

    // ── DaysUntilDueAt ───────────────────────────────────────────────────────

    [Fact]
    public void DaysUntilDueAt_OnBorrowDate_Returns14()
    {
        var loan = CreateActiveLoan();

        loan.DaysUntilDueAt(BorrowedAt).Should().Be(14);
    }

    [Fact]
    public void DaysUntilDueAt_ThreeDaysBeforeDue_Returns3()
    {
        var loan = CreateActiveLoan();
        var threeDaysBefore = DueDate.AddDays(-3);

        loan.DaysUntilDueAt(threeDaysBefore).Should().Be(3);
    }

    [Fact]
    public void DaysUntilDueAt_ExactlyAtDueDate_Returns0()
    {
        var loan = CreateActiveLoan();

        loan.DaysUntilDueAt(DueDate).Should().Be(0);
    }

    [Fact]
    public void DaysUntilDueAt_OneDayOverdue_ReturnsNegative1()
    {
        var loan = CreateActiveLoan();
        var oneDayAfter = DueDate.AddDays(1);

        loan.DaysUntilDueAt(oneDayAfter).Should().Be(-1);
    }

    [Fact]
    public void DaysUntilDueAt_FiveDaysOverdue_ReturnsNegative5()
    {
        var loan = CreateActiveLoan();
        var fiveDaysAfter = DueDate.AddDays(5);

        loan.DaysUntilDueAt(fiveDaysAfter).Should().Be(-5);
    }

    [Fact]
    public void DaysUntilDueAt_ReturnedLoan_Returns0()
    {
        var loan = CreateReturnedLoan();

        loan.DaysUntilDueAt(BorrowedAt).Should().Be(0);
    }

    // ── Convenience properties delegate correctly ────────────────────────────

    [Fact]
    public void IsOverdue_DelegatesToIsOverdueAt()
    {
        // A loan with DueDate far in the past will always be overdue via the property
        var pastBorrowed = DateTime.UtcNow.AddDays(-30);
        var loan = BookLoan.Create(ValidBookId, ValidUserId, pastBorrowed, 14);
        // DueDate = pastBorrowed + 14 days = 16 days ago → overdue

        loan.IsOverdue.Should().BeTrue();
    }

    [Fact]
    public void DaysUntilDue_DelegatesToDaysUntilDueAt()
    {
        // A loan created right now with 14-day duration should have ~14 days until due
        var loan = BookLoan.Create(ValidBookId, ValidUserId, DateTime.UtcNow, 14);

        loan.DaysUntilDue.Should().BeInRange(13, 14);
    }
}
