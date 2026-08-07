using MediatR;
using MiniLibrary.Application.Users.DTOs;
using MiniLibrary.Domain.Common;

namespace MiniLibrary.Application.Users.Queries.GetUsers;

/// <summary>
/// Query to retrieve a paginated list of all users (Admin only).
/// </summary>
public sealed record GetUsersQuery : IRequest<PagedResult<UserResponse>>
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}
