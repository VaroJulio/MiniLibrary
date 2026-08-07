using MediatR;
using MiniLibrary.Application.Dashboard.DTOs;
using MiniLibrary.Application.Interfaces;

namespace MiniLibrary.Application.Dashboard.Queries.GetLoanMetrics;

/// <summary>
/// Handles GetLoanMetricsQuery by computing loan metrics from repository data.
/// </summary>
public sealed class GetLoanMetricsQueryHandler
    : IRequestHandler<GetLoanMetricsQuery, LoanMetricsResponse>
{
    private readonly IDashboardService _dashboardService;

    public GetLoanMetricsQueryHandler(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<LoanMetricsResponse> Handle(
        GetLoanMetricsQuery request,
        CancellationToken cancellationToken)
    {
        return await _dashboardService.GetLoanMetricsAsync(cancellationToken);
    }
}
