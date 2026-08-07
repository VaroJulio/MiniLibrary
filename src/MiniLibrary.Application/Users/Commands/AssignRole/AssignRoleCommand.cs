using MediatR;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.Application.Users.Commands.AssignRole;

/// <summary>
/// Command to assign a role to a user. Admin only.
/// Prevents the sole Admin from changing their own role.
/// </summary>
public sealed record AssignRoleCommand(Guid UserId, UserRole NewRole) : IRequest<Unit>;
