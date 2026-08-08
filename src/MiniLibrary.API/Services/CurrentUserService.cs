using MiniLibrary.API.Extensions;
using MiniLibrary.Application.Interfaces;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.API.Services;

/// <summary>
/// Extracts the current authenticated user's information from the HTTP context claims.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? UserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId();

    public UserRole? Role =>
        _httpContextAccessor.HttpContext?.User.GetRole();

    public string? Email =>
        _httpContextAccessor.HttpContext?.User.GetEmail();

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated ?? false;
}
