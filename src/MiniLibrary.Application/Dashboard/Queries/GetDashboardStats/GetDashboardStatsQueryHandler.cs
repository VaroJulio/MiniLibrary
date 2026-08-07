using MediatR;
using Microsoft.Extensions.Logging;
using MiniLibrary.Application.Dashboard.DTOs;
using MiniLibrary.Application.Interfaces;

namespace MiniLibrary.Application.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// Handles GetDashboardStatsQuery by aggregating library-wide statistics.
/// </summary>
public sealed class GetDashboardStatsQueryHandler
    : IRequestHandler<GetDashboardStatsQuery, DashboardStatsResponse>
{
    private readonly IDashboardService _dashboardService;

    public GetDashboardStatsQueryHandler(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    public async Task<DashboardStatsResponse> Handle(
        GetDashboardStatsQuery request,
        CancellationToken cancellationToken)
    {
        return await _dashboardService.GetStatsAsync(cancellationToken);
    }
}
