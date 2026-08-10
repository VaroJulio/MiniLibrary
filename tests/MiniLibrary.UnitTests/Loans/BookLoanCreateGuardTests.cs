using MiniLibrary.Domain.Entities;

namespace MiniLibrary.UnitTests.Loans;

/// <summary>
/// Unit tests verifying BookLoan.Create guard clauses reject invalid arguments.
/// </summary>
public class BookLoanCreateGuardTests
{
    private static readonly Guid ValidBookId = Guid.NewGuid();
    private static readonly Guid ValidUserId = Guid.NewGuid();
    private static readonly DateTime ValidBorrowedAt = DateTime.UtcNow;

    [Fact]
    public void Create_EmptyBookId_ThrowsArgumentException()
    {
        var act = () => BookLoan.Create(Guid.Empty, ValidUserId, ValidBorrowedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("bookId")
            .WithMessage("*BookId is required*");
    }

    [Fact]
    public void Create_EmptyUserId_ThrowsArgumentException()
    {
        var act = () => BookLoan.Create(ValidBookId, Guid.Empty, ValidBorrowedAt);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("userId")
            .WithMessage("*UserId is required*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-14)]
    [InlineData(int.MinValue)]
    public void Create_NonPositiveLoanDuration_ThrowsArgumentOutOfRangeException(int invalidDays)
    {
        var act = () => BookLoan.Create(ValidBookId, ValidUserId, ValidBorrowedAt, invalidDays);

        act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("loanDurationDays");
    }

    [Fact]
    public void Create_ValidArguments_ReturnsBookLoan()
    {
        var loan = BookLoan.Create(ValidBookId, ValidUserId, ValidBorrowedAt, 14);

        loan.Should().NotBeNull();
        loan.BookId.Should().Be(ValidBookId);
        loan.UserId.Should().Be(ValidUserId);
        loan.BorrowedAt.Should().Be(ValidBorrowedAt);
        loan.DueDate.Should().Be(ValidBorrowedAt.AddDays(14));
        loan.ReturnedAt.Should().BeNull();
        loan.IsActive.Should().BeTrue();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    [InlineData(14)]
    [InlineData(30)]
    public void Create_PositiveLoanDuration_SetsDueDateCorrectly(int days)
    {
        var loan = BookLoan.Create(ValidBookId, ValidUserId, ValidBorrowedAt, days);

        loan.DueDate.Should().Be(ValidBorrowedAt.AddDays(days));
    }
}
