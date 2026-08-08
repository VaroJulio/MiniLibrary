using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.UnitTests.Authorization;

/// <summary>
/// Tests that user provisioning correctly creates users with Member role on first SSO login (Req 6.3).
/// </summary>
public class UserProvisioningTests
{
    [Fact]
    public void Create_NewUser_AssignsMemberRoleByDefault()
    {
        var user = User.Create(
            email: "newuser@example.com",
            fullName: "New User",
            externalId: "google-123",
            provider: "Google");

        user.Role.Should().Be(UserRole.Member);
    }

    [Fact]
    public void Create_NewUser_SetsEmailAndFullName()
    {
        var user = User.Create(
            email: "jane@example.com",
            fullName: "Jane Doe",
            externalId: "ms-456",
            provider: "Microsoft");

        user.Email.Should().Be("jane@example.com");
        user.FullName.Should().Be("Jane Doe");
    }

    [Fact]
    public void Create_NewUser_StoresExternalIdAndProvider()
    {
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-789",
            provider: "Google");

        user.ExternalId.Should().Be("ext-789");
        user.Provider.Should().Be("Google");
    }

    [Fact]
    public void Create_NewUser_IsNotDeleted()
    {
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-001",
            provider: "Google");

        user.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Create_NewUser_SetsCreatedAtToNow()
    {
        var before = DateTime.UtcNow;
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-002",
            provider: "Google");
        var after = DateTime.UtcNow;

        user.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void Create_NewUser_EnablesEmailAlertsExpiration()
    {
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-003",
            provider: "Google");

        user.EmailAlertsExpiration.Should().BeTrue();
    }

    [Fact]
    public void Create_NewUser_EnablesEmailAlertsAvailability()
    {
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-004",
            provider: "Google");

        user.EmailAlertsAvailability.Should().BeTrue();
    }

    [Fact]
    public void AssignRole_UpdatesRole_AndTimestamp()
    {
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-005",
            provider: "Google");

        var originalUpdatedAt = user.UpdatedAt;
        Thread.Sleep(10); // Ensure timestamp difference
        user.AssignRole(UserRole.Librarian);

        user.Role.Should().Be(UserRole.Librarian);
        user.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [Fact]
    public void AssignRole_CanElevateToAdmin()
    {
        var user = User.Create(
            email: "admin@example.com",
            fullName: "Admin User",
            externalId: "ext-006",
            provider: "Google");

        user.AssignRole(UserRole.Admin);

        user.Role.Should().Be(UserRole.Admin);
    }

    [Fact]
    public void AssignRole_CanDowngradeToMember()
    {
        var user = User.Create(
            email: "user@example.com",
            fullName: "Test User",
            externalId: "ext-007",
            provider: "Google",
            role: UserRole.Librarian);

        user.AssignRole(UserRole.Member);

        user.Role.Should().Be(UserRole.Member);
    }
}
