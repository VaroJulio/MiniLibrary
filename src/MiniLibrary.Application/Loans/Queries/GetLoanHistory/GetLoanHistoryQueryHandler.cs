using MediatR;
using MiniLibrary.Application.Loans.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Loans.Queries.GetLoanHistory;

/// <summary>
/// Handles retrieval of a user's loan history with pagination.
/// Returns loans ordered by BorrowedAt descending (most recent first).
/// </summary>
public class GetLoanHistoryQueryHandler : IRequestHandler<GetLoanHistoryQuery, PagedResult<LoanResponse>>
{
    private readonly ILoanRepository _loanRepository;

    public GetLoanHistoryQueryHandler(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<PagedResult<LoanResponse>> Handle(GetLoanHistoryQuery request, CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);

        var result = await _loanRepository.GetUserHistoryAsync(request.UserId, paging, cancellationToken);

        var loanResponses = result.Items.Select(loan => new LoanResponse(
            Id: loan.Id,
            BookId: loan.BookId,
            BookTitle: loan.Book.Title,
            BorrowedAt: loan.BorrowedAt,
            DueDate: loan.DueDate,
            ReturnedAt: loan.ReturnedAt)).ToList();

        return new PagedResult<LoanResponse>(loanResponses, result.TotalCount, result.Page, result.PageSize);
    }
}
