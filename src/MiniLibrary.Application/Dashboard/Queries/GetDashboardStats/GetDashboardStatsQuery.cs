using MediatR;
using MiniLibrary.Application.Dashboard.DTOs;

namespace MiniLibrary.Application.Dashboard.Queries.GetDashboardStats;

/// <summary>
/// Query to retrieve overview dashboard statistics (Librarian, Admin).
/// </summary>
public sealed record GetDashboardStatsQuery : IRequest<DashboardStatsResponse>;
