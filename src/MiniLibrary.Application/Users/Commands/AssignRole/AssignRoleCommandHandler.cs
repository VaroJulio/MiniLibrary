using MediatR;
using MiniLibrary.Application.Common.Exceptions;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Domain.Interfaces;

namespace MiniLibrary.Application.Users.Commands.AssignRole;

/// <summary>
/// Handles AssignRoleCommand by updating the user's role.
/// Validates: user exists, prevents sole Admin from losing Admin role.
/// </summary>
public sealed class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Unit>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleCommandHandler(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Unit> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);
        if (user is null)
        {
            throw new NotFoundException("User", request.UserId);
        }

        // Prevent sole Admin from changing own role (Req 7.3)
        if (user.Role == UserRole.Admin && request.NewRole != UserRole.Admin)
        {
            var adminCount = await _userRepository.GetAdminCountAsync(cancellationToken);
            if (adminCount <= 1)
            {
                throw new ConflictException("Cannot change the role of the sole Admin. At least one Admin must exist.");
            }
        }

        user.AssignRole(request.NewRole);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}
