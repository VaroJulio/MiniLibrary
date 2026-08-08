using MediatR;
using MiniLibrary.Domain.Common;
using MiniLibrary.Application.Loans.DTOs;

namespace MiniLibrary.Application.Loans.Queries.GetOverdueLoans;

/// <summary>
/// Query to retrieve paginated list of overdue loans.
/// Only accessible by Librarian and Admin roles.
/// Returns loans whose DueDate has passed and have not been returned.
/// </summary>
public record GetOverdueLoansQuery(int Page = 1, int PageSize = 20) : IRequest<PagedResult<OverdueLoanResponse>>;
