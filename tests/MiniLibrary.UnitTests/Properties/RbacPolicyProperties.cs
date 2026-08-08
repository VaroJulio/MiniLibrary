using System.Security.Claims;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using MiniLibrary.API.Configuration;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property 8: Role-Based Access Control —
/// Generate random (role, policy) pairs and verify correct authorization decisions.
/// Validates: Requirements 1.9, 6.6, 7.5, 8.5, 16.2, 17.7, 20.9
/// </summary>
public class RbacPolicyProperties
{
    /// <summary>
    /// The expected permission matrix from Requirements 7.4:
    /// | Policy           | Admin | Librarian | Member |
    /// |------------------|-------|-----------|--------|
    /// | AdminOnly        | Yes   | No        | No     |
    /// | LibrarianOrAdmin | Yes   | Yes       | No     |
    /// | MemberOnly       | Yes   | Yes       | Yes    |
    /// | Authenticated    | Yes   | Yes       | Yes    |
    /// </summary>
    private static readonly Dictionary<(UserRole Role, string Policy), bool> ExpectedDecisions = new()
    {
        // Admin can access everything
        { (UserRole.Admin, AuthorizationConfig.Policies.AdminOnly), true },
        { (UserRole.Admin, AuthorizationConfig.Policies.LibrarianOrAdmin), true },
        { (UserRole.Admin, AuthorizationConfig.Policies.MemberOnly), true },
        { (UserRole.Admin, AuthorizationConfig.Policies.Authenticated), true },

        // Librarian can access LibrarianOrAdmin and below
        { (UserRole.Librarian, AuthorizationConfig.Policies.AdminOnly), false },
        { (UserRole.Librarian, AuthorizationConfig.Policies.LibrarianOrAdmin), true },
        { (UserRole.Librarian, AuthorizationConfig.Policies.MemberOnly), true },
        { (UserRole.Librarian, AuthorizationConfig.Policies.Authenticated), true },

        // Member can access MemberOnly and Authenticated only
        { (UserRole.Member, AuthorizationConfig.Policies.AdminOnly), false },
        { (UserRole.Member, AuthorizationConfig.Policies.LibrarianOrAdmin), false },
        { (UserRole.Member, AuthorizationConfig.Policies.MemberOnly), true },
        { (UserRole.Member, AuthorizationConfig.Policies.Authenticated), true },
    };

    private static readonly string[] AllPolicies =
    [
        AuthorizationConfig.Policies.AdminOnly,
        AuthorizationConfig.Policies.LibrarianOrAdmin,
        AuthorizationConfig.Policies.MemberOnly,
        AuthorizationConfig.Policies.Authenticated,
    ];

    private static IAuthorizationService BuildAuthorizationService()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAuthorizationPolicies();
        services.AddSingleton<IAuthorizationHandler, PassThroughAuthorizationHandler>();

        var provider = services.BuildServiceProvider();
        return provider.GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal CreatePrincipalForRole(UserRole role)
    {
        var claims = new List<Claim>
        {
            new("sub", Guid.NewGuid().ToString()),
            new("email", $"{role.ToString().ToLower()}@test.com"),
            new("role", role.ToString()),
        };

        var identity = new ClaimsIdentity(claims, "TestAuth", "email", "role");
        return new ClaimsPrincipal(identity);
    }

    /// <summary>
    /// FsCheck generator that produces random (UserRole, PolicyName) pairs.
    /// </summary>
    public static Arbitrary<(UserRole Role, string Policy)> RolePolicyPairArbitrary()
    {
        var gen = from role in Gen.Elements(UserRole.Admin, UserRole.Librarian, UserRole.Member)
                  from policy in Gen.Elements(AllPolicies)
                  select (Role: role, Policy: policy);
        return Arb.From(gen);
    }

    [Property(Arbitrary = new[] { typeof(RbacPolicyProperties) }, MaxTest = 100)]
    public async void RolePolicyDecision_AlwaysMatchesPermissionMatrix(
        (UserRole Role, string Policy) input)
    {
        // Arrange
        var authService = BuildAuthorizationService();
        var principal = CreatePrincipalForRole(input.Role);
        var expectedAllowed = ExpectedDecisions[(input.Role, input.Policy)];

        // Act
        var result = await authService.AuthorizeAsync(principal, null, input.Policy);

        // Assert
        result.Succeeded.Should().Be(expectedAllowed,
            because: $"Role '{input.Role}' accessing policy '{input.Policy}' " +
                     $"should be {(expectedAllowed ? "allowed" : "denied")} per permission matrix (Req 7.4)");
    }

    [Property(Arbitrary = new[] { typeof(RbacPolicyProperties) }, MaxTest = 50)]
    public async void AdminRole_CanAccessAllPolicies((UserRole Role, string Policy) input)
    {
        // This property focuses only on Admin to verify Admin always passes
        if (input.Role != UserRole.Admin) return;

        var authService = BuildAuthorizationService();
        var principal = CreatePrincipalForRole(UserRole.Admin);

        var result = await authService.AuthorizeAsync(principal, null, input.Policy);

        result.Succeeded.Should().BeTrue(
            because: $"Admin role should have access to all policies including '{input.Policy}' (Req 7.4)");
    }

    [Fact]
    public async Task UnauthenticatedUser_DeniedAllPolicies()
    {
        // Arrange — unauthenticated user (no identity)
        var authService = BuildAuthorizationService();
        var principal = new ClaimsPrincipal(new ClaimsIdentity()); // not authenticated

        // Act & Assert — every policy should deny
        foreach (var policy in AllPolicies)
        {
            var result = await authService.AuthorizeAsync(principal, null, policy);
            result.Succeeded.Should().BeFalse(
                because: $"Unauthenticated users should be denied for policy '{policy}' (Req 6.6)");
        }
    }

    [Theory]
    [InlineData(UserRole.Member, AuthorizationConfig.Policies.AdminOnly)]
    [InlineData(UserRole.Member, AuthorizationConfig.Policies.LibrarianOrAdmin)]
    [InlineData(UserRole.Librarian, AuthorizationConfig.Policies.AdminOnly)]
    public async Task LowerRole_DeniedHigherPolicies(UserRole role, string policy)
    {
        // Arrange
        var authService = BuildAuthorizationService();
        var principal = CreatePrincipalForRole(role);

        // Act
        var result = await authService.AuthorizeAsync(principal, null, policy);

        // Assert
        result.Succeeded.Should().BeFalse(
            because: $"Role '{role}' should NOT have access to '{policy}' (Req 7.5)");
    }

    [Theory]
    [InlineData(UserRole.Admin)]
    [InlineData(UserRole.Librarian)]
    [InlineData(UserRole.Member)]
    public async Task AuthenticatedUser_AlwaysPassesAuthenticatedPolicy(UserRole role)
    {
        // Arrange
        var authService = BuildAuthorizationService();
        var principal = CreatePrincipalForRole(role);

        // Act
        var result = await authService.AuthorizeAsync(
            principal, null, AuthorizationConfig.Policies.Authenticated);

        // Assert
        result.Succeeded.Should().BeTrue(
            because: $"Any authenticated user (role='{role}') should pass the Authenticated policy");
    }
}
