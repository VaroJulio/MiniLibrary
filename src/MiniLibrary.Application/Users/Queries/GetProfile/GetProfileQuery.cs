using MediatR;
using MiniLibrary.Application.Users.DTOs;

namespace MiniLibrary.Application.Users.Queries.GetProfile;

/// <summary>
/// Query to retrieve the current authenticated user's profile.
/// </summary>
public sealed record GetProfileQuery(Guid UserId) : IRequest<UserProfileResponse?>;
