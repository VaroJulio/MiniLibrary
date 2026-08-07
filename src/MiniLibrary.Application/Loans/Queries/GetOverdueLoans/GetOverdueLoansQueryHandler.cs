using MediatR;
using MiniLibrary.Application.Loans.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Loans.Queries.GetOverdueLoans;

/// <summary>
/// Handles retrieval of overdue loans with pagination.
/// Returns loans whose DueDate is before now and have not been returned,
/// including member and book information.
/// </summary>
public class GetOverdueLoansQueryHandler : IRequestHandler<GetOverdueLoansQuery, PagedResult<OverdueLoanResponse>>
{
    private readonly ILoanRepository _loanRepository;

    public GetOverdueLoansQueryHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<PagedResult<OverdueLoanResponse>> Handle(GetOverdueLoansQuery request, CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);

        var result = await _loanRepository.GetOverdueLoansAsync(paging, cancellationToken);

        var now = DateTime.UtcNow;

        var overdueResponses = result.Items.Select(loan => new OverdueLoanResponse(
            Id: loan.Id,
            BookId: loan.BookId,
            BookTitle: loan.Book.Title,
            UserId: loan.UserId,
            UserName: loan.User.FullName,
            UserEmail: loan.User.Email,
            BorrowedAt: loan.BorrowedAt,
            DueDate: loan.DueDate,
            DaysOverdue: (int)(now - loan.DueDate).TotalDays)).ToList();

        return new PagedResult<OverdueLoanResponse>(overdueResponses, result.TotalCount, result.Page, result.PageSize);
    }
}
