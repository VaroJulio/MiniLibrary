using MediatR;
using MiniLibrary.Application.Users.DTOs;
using MiniLibrary.Domain.Common;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Users.Queries.GetUsers;

/// <summary>
/// Handles GetUsersQuery by returning a paginated list of all users.
/// </summary>
public sealed class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, PagedResult<UserResponse>>
{
    private readonly IUserRepository _userRepository;

    public GetUsersQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<PagedResult<UserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var paging = new PaginationParams(request.Page, request.PageSize);
        var result = await _userRepository.GetAllAsync(paging, cancellationToken);

        var userResponses = result.Items.Select(u => new UserResponse(
            u.Id,
            u.Email,
            u.FullName,
            u.Role.ToString(),
            u.CreatedAt)).ToList();

        return new PagedResult<UserResponse>(
            userResponses,
            result.TotalCount,
            result.Page,
            result.PageSize);
    }
}
