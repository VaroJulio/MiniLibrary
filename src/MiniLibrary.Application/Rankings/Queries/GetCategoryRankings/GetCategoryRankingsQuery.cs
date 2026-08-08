using MediatR;
using MiniLibrary.Application.Rankings.DTOs;

namespace MiniLibrary.Application.Rankings.Queries.GetCategoryRankings;

/// <summary>
/// Query to retrieve category rankings with best-rated book per category.
/// </summary>
public sealed record GetCategoryRankingsQuery : IRequest<List<CategoryRankingItem>>;
