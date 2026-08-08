using MediatR;
using MiniLibrary.Application.Users.DTOs;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Users.Queries.GetProfile;

/// <summary>
/// Handles GetProfileQuery by returning the user's profile details.
/// </summary>
public sealed class GetProfileQueryHandler : IRequestHandler<GetProfileQuery, UserProfileResponse?>
{
    private readonly IUserRepository _userRepository;

    public GetProfileQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserProfileResponse?> Handle(GetProfileQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
            return null;

        return new UserProfileResponse(
            user.Id,
            user.Email,
            user.FullName,
            user.Role.ToString(),
            user.EmailAlertsExpiration,
            user.EmailAlertsAvailability,
            user.CreatedAt);
    }
}
