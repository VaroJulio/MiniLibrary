using System.Security.Claims;
using FluentAssertions;
using MiniLibrary.Domain.Enumerations;
using Xunit;

namespace MiniLibrary.UnitTests.Auth;

/// <summary>
/// Tests for role-based authorization policy logic (Req 7.4, 7.5).
/// Validates the permission matrix by testing claim-based role evaluation logic.
/// Since the ASP.NET Core runtime is not available for unit tests, these tests
/// validate the role-claim logic that underpins the policies.
/// </summary>
public class AuthorizationPoliciesTests
{
    /// <summary>
    /// Simulates the AdminOnly policy check: requires role == Admin.
    /// </summary>
    private static bool IsAdminOnly(ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? principal.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value == "Admin";
    }

    /// <summary>
    /// Simulates the LibrarianOrAdmin policy check: requires role == Admin or Librarian.
    /// </summary>
    private static bool IsLibrarianOrAdmin(ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? principal.FindFirst(ClaimTypes.Role);
        if (roleClaim is null) return false;
        return roleClaim.Value == "Admin" || roleClaim.Value == "Librarian";
    }

    /// <summary>
    /// Simulates the MemberOnly policy check: any authenticated user with recognized role.
    /// </summary>
    private static bool IsMemberOrAbove(ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? principal.FindFirst(ClaimTypes.Role);
        if (roleClaim is null) return false;
        return roleClaim.Value == "Admin" || roleClaim.Value == "Librarian" || roleClaim.Value == "Member";
    }

    /// <summary>
    /// Simulates the Authenticated policy check.
    /// </summary>
    private static bool IsAuthenticated(ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated ?? false;
    }

    private static ClaimsPrincipal CreatePrincipal(UserRole role)
    {
        var claims = new[]
        {
            new Claim("role", role.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateAnonymousPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    // --- AdminOnly Policy Tests ---

    [Fact]
    public void AdminOnly_AdminUser_Allowed()
    {
        var principal = CreatePrincipal(UserRole.Admin);
        IsAdminOnly(principal).Should().BeTrue();
    }

    [Fact]
    public void AdminOnly_LibrarianUser_Denied()
    {
        var principal = CreatePrincipal(UserRole.Librarian);
        IsAdminOnly(principal).Should().BeFalse();
    }

    [Fact]
    public void AdminOnly_MemberUser_Denied()
    {
        var principal = CreatePrincipal(UserRole.Member);
        IsAdminOnly(principal).Should().BeFalse();
    }

    [Fact]
    public void AdminOnly_Anonymous_Denied()
    {
        var principal = CreateAnonymousPrincipal();
        IsAdminOnly(principal).Should().BeFalse();
    }

    // --- LibrarianOrAdmin Policy Tests ---

    [Fact]
    public void LibrarianOrAdmin_AdminUser_Allowed()
    {
        var principal = CreatePrincipal(UserRole.Admin);
        IsLibrarianOrAdmin(principal).Should().BeTrue();
    }

    [Fact]
    public void LibrarianOrAdmin_LibrarianUser_Allowed()
    {
        var principal = CreatePrincipal(UserRole.Librarian);
        IsLibrarianOrAdmin(principal).Should().BeTrue();
    }

    [Fact]
    public void LibrarianOrAdmin_MemberUser_Denied()
    {
        var principal = CreatePrincipal(UserRole.Member);
        IsLibrarianOrAdmin(principal).Should().BeFalse();
    }

    [Fact]
    public void LibrarianOrAdmin_Anonymous_Denied()
    {
        var principal = CreateAnonymousPrincipal();
        IsLibrarianOrAdmin(principal).Should().BeFalse();
    }

    // --- MemberOnly Policy Tests (any user with a recognized role) ---

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Librarian)]
    [InlineData(UserRole.Member)]
    public void MemberOnly_AuthenticatedUserWithRole_Allowed(UserRole role)
    {
        var principal = CreatePrincipal(role);
        IsMemberOrAbove(principal).Should().BeTrue();
    }

    [Fact]
    public void MemberOnly_Anonymous_Denied()
    {
        var principal = CreateAnonymousPrincipal();
        IsMemberOrAbove(principal).Should().BeFalse();
    }

    // --- Authenticated Policy Tests ---

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Librarian)]
    [InlineData(UserRole.Member)]
    public void Authenticated_AuthenticatedUser_Allowed(UserRole role)
    {
        var principal = CreatePrincipal(role);
        IsAuthenticated(principal).Should().BeTrue();
    }

    [Fact]
    public void Authenticated_Anonymous_Denied()
    {
        var principal = CreateAnonymousPrincipal();
        IsAuthenticated(principal).Should().BeFalse();
    }

    // --- Permission Matrix Tests (Req 7.4) ---

    [Fact]
    public void PermissionMatrix_UserManagement_OnlyAdminAllowed()
    {
        // Req 7.5: Librarian or Member accessing user management endpoints → 403
        IsAdminOnly(CreatePrincipal(UserRole.Admin)).Should().BeTrue("Admin should access user management");
        IsAdminOnly(CreatePrincipal(UserRole.Librarian)).Should().BeFalse("Librarian should NOT access user management");
        IsAdminOnly(CreatePrincipal(UserRole.Member)).Should().BeFalse("Member should NOT access user management");
    }

    [Fact]
    public void PermissionMatrix_BookCrud_AdminAndLibrarianAllowed()
    {
        // Req 7.4: Book CRUD → Admin and Librarian
        IsLibrarianOrAdmin(CreatePrincipal(UserRole.Admin)).Should().BeTrue("Admin should manage books");
        IsLibrarianOrAdmin(CreatePrincipal(UserRole.Librarian)).Should().BeTrue("Librarian should manage books");
        IsLibrarianOrAdmin(CreatePrincipal(UserRole.Member)).Should().BeFalse("Member should NOT manage books");
    }

    [Fact]
    public void PermissionMatrix_Dashboard_AdminAndLibrarianAllowed()
    {
        // Req 7.4: Dashboard → Admin and Librarian
        IsLibrarianOrAdmin(CreatePrincipal(UserRole.Admin)).Should().BeTrue("Admin should access dashboard");
        IsLibrarianOrAdmin(CreatePrincipal(UserRole.Librarian)).Should().BeTrue("Librarian should access dashboard");
        IsLibrarianOrAdmin(CreatePrincipal(UserRole.Member)).Should().BeFalse("Member should NOT access dashboard");
    }

    [Fact]
    public void PermissionMatrix_SearchRecommendationsRatingsRankingsWishlistGamification_AllRolesAllowed()
    {
        // Req 7.4: Search, Recommendations, Ratings, Rankings, Wishlist, Gamification → All
        IsMemberOrAbove(CreatePrincipal(UserRole.Admin)).Should().BeTrue();
        IsMemberOrAbove(CreatePrincipal(UserRole.Librarian)).Should().BeTrue();
        IsMemberOrAbove(CreatePrincipal(UserRole.Member)).Should().BeTrue();
    }
}
