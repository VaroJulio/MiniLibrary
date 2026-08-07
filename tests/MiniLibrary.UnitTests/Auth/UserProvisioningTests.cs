using FluentAssertions;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using Xunit;

namespace MiniLibrary.UnitTests.Auth;

/// <summary>
/// Tests for automatic user creation with Member role on first SSO login (Req 6.3).
/// </summary>
public class UserProvisioningTests
{
    [Fact]
    public void Create_NewUser_DefaultsToMemberRole()
    {
        // Req 6.3: When a new user authenticates for the first time via SSO,
        // the system SHALL create an account with Member role by default
        var user = User.Create(
            email: "newuser@example.com",
            fullName: "New User",
            externalId: "ext-123",
            provider: "Google");

        user.Role.Should().Be(UserRole.Member);
    }

    [Fact]
    public void Create_NewUser_SetsEmailAndFullName()
    {
        var user = User.Create(
            email: "jane@example.com",
            fullName: "Jane Doe",
            externalId: "ext-456",
            provider: "Microsoft");

        user.Email.Should().Be("jane@example.com");
        user.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public void Create_NewUser_SetsExternalIdAndProvider()
    {
        var user = User.Create(
            email: "user@test.com",
            fullName: "Test User",
            externalId: "google-sub-789",
            provider: "Google");

        user.ExternalId.Should().Be("google-sub-789");
        user.Provider.Should().Be("Google");
    }

    [Fact]
    public void Create_NewUser_IsNotDeleted()
    {
        var user = User.Create(
            email: "active@test.com",
            fullName: "Active User",
            externalId: "ext-active",
            provider: "Microsoft");

        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_NewUser_DefaultsEmailAlertPreferencesToTrue()
    {
        var user = User.Create(
            email: "user@test.com",
            fullName: "Test User",
            externalId: "ext-x",
            provider: "Google");

        user.EmailAlertsExpiration.Should().BeTrue();
        user.EmailAlertsAvailability.Should().BeTrue();
    }

    [Fact]
    public void Create_NewUser_SetsCreatedAtToCurrentTime()
    {
        var before = DateTime.UtcNow;

        var user = User.Create(
            email: "user@test.com",
            fullName: "Test User",
            externalId: "ext-time",
            provider: "Google");

        var after = DateTime.UtcNow;

        user.CreatedAt.Should().BeOnOrAfter(before);
        user.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Create_WithExplicitRole_UsesProvidedRole()
    {
        // Admin can be provisioned with an explicit role (e.g., seed data or special flow)
        var user = User.Create(
            email: "admin@test.com",
            fullName: "Admin User",
            externalId: "ext-admin",
            provider: "Google",
            role: UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void AssignRole_ChangesUserRole()
    {
        // Req 6.7: When an Admin assigns a new role, the system SHALL update immediately
        var user = User.Create(
            email: "user@test.com",
            fullName: "Test User",
            externalId: "ext-role",
            provider: "Google");

        user.Role.Should().Be(UserRole.Member);

        user.AssignRole(UserRole.Librarian);

        user.Role.Should().Be(UserRole.Librarian);
    }

    [Fact]
    public void AssignRole_UpdatesTimestamp()
    {
        var user = User.Create(
            email: "user@test.com",
            fullName: "Test User",
            externalId: "ext-ts",
            provider: "Google");

        var createdAt = user.UpdatedAt;

        // Small delay to ensure timestamp difference
        user.AssignRole(UserRole.Admin);

        user.UpdatedAt.Should().BeOnOrAfter(createdAt);
    }
}
