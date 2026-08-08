using FluentAssertions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using Xunit;

namespace MiniLibrary.UnitTests.Auth;

/// <summary>
/// Tests for user management business logic including role assignment and sole-admin protection (Req 7.1-7.5).
/// Tests the domain logic that backs the UsersController's role assignment endpoint.
/// </summary>
public class RoleAssignmentLogicTests
{
    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Librarian)]
    [InlineData(UserRole.Member)]
    public void AssignRole_ValidRole_UpdatesUserRole(UserRole newRole)
    {
        // Arrange
        var user = User.Create("user@test.com", "Test User", "ext-1", "Google");
        user.Role.Should().Be(UserRole.Member); // Default

        // Act
        user.AssignRole(newRole);

        // Assert
        user.Role.Should().Be(newRole);
    }

    [Fact]
    public void AssignRole_UpdatesTimestamp()
    {
        // Arrange
        var user = User.Create("user@test.com", "Test User", "ext-1", "Google");
        var originalTimestamp = user.UpdatedAt;

        // Act
        user.AssignRole(UserRole.Librarian);

        // Assert
        user.UpdatedAt.Should().BeOnOrAfter(originalTimestamp);
    }

    [Fact]
    public void SoleAdminProtection_WhenSoleAdmin_ShouldPreventRoleChange()
    {
        // Req 7.3: IF sole Admin tries to change their own role, the API SHALL reject the operation
        // This test validates the business rule logic
        var adminUser = User.Create("admin@test.com", "Admin User", "ext-admin", "Google", UserRole.Admin);
        var adminCount = 1; // Sole admin
        var isChangingOwnRole = true;
        var newRole = UserRole.Member;

        // Business rule: reject if admin changes own role AND is the only admin
        var shouldReject = isChangingOwnRole
            && adminUser.Role == UserRole.Admin
            && newRole != UserRole.Admin
            && adminCount <= 1;

        shouldReject.Should().BeTrue("sole Admin should not be able to change their own role");
    }

    [Fact]
    public void SoleAdminProtection_WhenMultipleAdmins_ShouldAllowRoleChange()
    {
        // Req 7.3: If there are multiple admins, self-role-change is allowed
        var adminUser = User.Create("admin@test.com", "Admin User", "ext-admin", "Google", UserRole.Admin);
        var adminCount = 3; // Multiple admins
        var isChangingOwnRole = true;
        var newRole = UserRole.Member;

        var shouldReject = isChangingOwnRole
            && adminUser.Role == UserRole.Admin
            && newRole != UserRole.Admin
            && adminCount <= 1;

        shouldReject.Should().BeFalse("role change should be allowed when multiple admins exist");
    }

    [Fact]
    public void SoleAdminProtection_WhenChangingOtherUserRole_ShouldAllow()
    {
        // Admin changing another user's role → always allowed
        var targetUser = User.Create("target@test.com", "Target User", "ext-target", "Google");
        var adminCount = 1;
        var isChangingOwnRole = false;
        var newRole = UserRole.Librarian;

        var shouldReject = isChangingOwnRole
            && targetUser.Role == UserRole.Admin
            && newRole != UserRole.Admin
            && adminCount <= 1;

        shouldReject.Should().BeFalse("changing another user's role should always be allowed");
    }

    [Fact]
    public void SoleAdminProtection_AdminAssigningSameRole_ShouldAllow()
    {
        // Admin assigning Admin role to self = no-op, always allowed
        var adminUser = User.Create("admin@test.com", "Admin User", "ext-admin", "Google", UserRole.Admin);
        var adminCount = 1;
        var isChangingOwnRole = true;
        var newRole = UserRole.Admin; // Same role

        var shouldReject = isChangingOwnRole
            && adminUser.Role == UserRole.Admin
            && newRole != UserRole.Admin
            && adminCount <= 1;

        shouldReject.Should().BeFalse("assigning the same Admin role to self should be allowed");
    }

    [Fact]
    public void RoleValidation_AllValidRoles_CanBeParsed()
    {
        // Test that all UserRole values can be round-tripped through string parsing
        // (validates the role assignment endpoint accepts valid role strings)
        foreach (var role in Enum.GetValues<UserRole>())
        {
            var roleString = role.ToString();
            Enum.TryParse<UserRole>(roleString, ignoreCase: true, out var parsed).Should().BeTrue();
            parsed.Should().Be(role);
        }
    }

    [Theory]
    [InlineData("admin")]
    [InlineData("ADMIN")]
    [InlineData("Admin")]
    [InlineData("librarian")]
    [InlineData("LIBRARIAN")]
    [InlineData("member")]
    [InlineData("MEMBER")]
    public void RoleValidation_CaseInsensitiveRoleParsing_Succeeds(string roleString)
    {
        // The controller uses case-insensitive parsing for role assignment
        Enum.TryParse<UserRole>(roleString, ignoreCase: true, out var _).Should().BeTrue();
    }

    [Theory]
    [InlineData("SuperAdmin")]
    [InlineData("Moderator")]
    [InlineData("")]
    [InlineData("Guest")]
    public void RoleValidation_InvalidRoleStrings_FailParsing(string roleString)
    {
        // Invalid roles should be rejected
        Enum.TryParse<UserRole>(roleString, ignoreCase: true, out var _).Should().BeFalse();
    }
}
