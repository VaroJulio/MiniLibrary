using MediatR;
using MiniLibrary.Application.Dashboard.DTOs;

namespace MiniLibrary.Application.Dashboard.Queries.GetLoanMetrics;

/// <summary>
/// Query to retrieve loan metrics: loans by period, popular categories, top books (Librarian, Admin).
/// </summary>
public sealed record GetLoanMetricsQuery : IRequest<LoanMetricsResponse>;
