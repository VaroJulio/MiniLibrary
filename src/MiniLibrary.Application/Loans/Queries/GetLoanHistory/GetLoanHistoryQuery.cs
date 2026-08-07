using MediatR;
using MiniLibrary.Domain.Common;
using MiniLibrary.Application.Loans.DTOs;

namespace MiniLibrary.Application.Loans.Queries.GetLoanHistory;

/// <summary>
/// Query to retrieve the paginated loan history for a specific user.
/// Returns loans ordered by date descending, 20 per page by default.
/// </summary>
public record GetLoanHistoryQuery(Guid UserId, int Page = 1, int PageSize = 20) : IRequest<PagedResult<LoanResponse>>;
