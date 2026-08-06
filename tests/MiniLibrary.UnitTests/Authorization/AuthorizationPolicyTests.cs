using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using MiniLibrary.API.Configuration;

namespace MiniLibrary.UnitTests.Authorization;

/// <summary>
/// Tests that authorization policies correctly evaluate role claims as per the permission matrix (Req 7.4).
/// </summary>
public class AuthorizationPolicyTests
{
    private readonly IAuthorizationService _authorizationService;

    public AuthorizationPolicyTests()
    {
        var services = new ServiceCollection();
        services.AddAuthorizationPolicies();
        services.AddLogging();

        var serviceProvider = services.BuildServiceProvider();
        _authorizationService = serviceProvider.GetRequiredService<IAuthorizationService>();
    }

    private static ClaimsPrincipal CreatePrincipal(string role)
    {
        var claims = new[]
        {
            new Claim("role", role),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestScheme");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateAnonymousPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity()); // No authenticated identity
    }

    // --- AdminOnly Policy ---

    [Fact]
    public async Task AdminOnly_Allows_Admin()
    {
        var user = CreatePrincipal("Admin");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.AdminOnly);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task AdminOnly_Denies_Librarian()
    {
        var user = CreatePrincipal("Librarian");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.AdminOnly);
        result.Succeeded.Should().BeFalse();
    }

    [Fact]
    public async Task AdminOnly_Denies_Member()
    {
        var user = CreatePrincipal("Member");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.AdminOnly);
        result.Succeeded.Should().BeFalse();
    }

    // --- LibrarianOrAdmin Policy ---

    [Fact]
    public async Task LibrarianOrAdmin_Allows_Admin()
    {
        var user = CreatePrincipal("Admin");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.LibrarianOrAdmin);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task LibrarianOrAdmin_Allows_Librarian()
    {
        var user = CreatePrincipal("Librarian");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.LibrarianOrAdmin);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task LibrarianOrAdmin_Denies_Member()
    {
        var user = CreatePrincipal("Member");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.LibrarianOrAdmin);
        result.Succeeded.Should().BeFalse();
    }

    // --- MemberOnly Policy (any authenticated role) ---

    [Fact]
    public async Task MemberOnly_Allows_Admin()
    {
        var user = CreatePrincipal("Admin");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.MemberOnly);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task MemberOnly_Allows_Librarian()
    {
        var user = CreatePrincipal("Librarian");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.MemberOnly);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task MemberOnly_Allows_Member()
    {
        var user = CreatePrincipal("Member");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.MemberOnly);
        result.Succeeded.Should().BeTrue();
    }

    // --- Authenticated Policy ---

    [Fact]
    public async Task Authenticated_Allows_AnyAuthenticatedUser()
    {
        var user = CreatePrincipal("Member");
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.Authenticated);
        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public async Task Authenticated_Denies_AnonymousUser()
    {
        var user = CreateAnonymousPrincipal();
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.Authenticated);
        result.Succeeded.Should().BeFalse();
    }

    // --- Permission Matrix Integration (Req 7.4) ---

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Librarian", false)]
    [InlineData("Member", false)]
    public async Task UserManagement_AdminOnly(string role, bool expectedAccess)
    {
        var user = CreatePrincipal(role);
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.AdminOnly);
        result.Succeeded.Should().Be(expectedAccess);
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Librarian", true)]
    [InlineData("Member", false)]
    public async Task BookManagement_LibrarianOrAdmin(string role, bool expectedAccess)
    {
        var user = CreatePrincipal(role);
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.LibrarianOrAdmin);
        result.Succeeded.Should().Be(expectedAccess);
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Librarian", true)]
    [InlineData("Member", false)]
    public async Task Dashboard_LibrarianOrAdmin(string role, bool expectedAccess)
    {
        var user = CreatePrincipal(role);
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.LibrarianOrAdmin);
        result.Succeeded.Should().Be(expectedAccess);
    }

    [Theory]
    [InlineData("Admin", true)]
    [InlineData("Librarian", true)]
    [InlineData("Member", true)]
    public async Task SearchAndRecommendations_AllRoles(string role, bool expectedAccess)
    {
        var user = CreatePrincipal(role);
        var result = await _authorizationService.AuthorizeAsync(user, AuthorizationConfig.Policies.MemberOnly);
        result.Succeeded.Should().Be(expectedAccess);
    }
}
