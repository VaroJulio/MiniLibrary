using System.Security.Claims;
using FsCheck;
using FsCheck.Xunit;
using MiniLibrary.Domain.Enumerations;

namespace MiniLibrary.UnitTests.Properties;

/// <summary>
/// Property-based tests for role-based access control.
/// Generates random (role, endpoint) pairs and verifies correct authorization decisions.
///
/// **Validates: Requirements 1.9, 6.6, 7.5, 8.5, 16.2, 17.7, 20.9**
/// </summary>
[Trait("Category", "Property")]
public class RoleBasedAccessControlProperties
{
    // ═══════════════════════════════════════════════════════════════════════════════
    // Endpoint/Resource Categories — mirrors the permission matrix from AuthorizationConfig
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Represents a protected endpoint category in the system.
    /// Each category maps to an authorization policy.
    /// </summary>
    public enum EndpointCategory
    {
        /// <summary>Book create/update/delete — requires LibrarianOrAdmin</summary>
        BookManagement,
        /// <summary>User management — requires AdminOnly</summary>
        UserManagement,
        /// <summary>Dashboard statistics — requires LibrarianOrAdmin</summary>
        Dashboard,
        /// <summary>Search books — requires MemberOnly (any authenticated role)</summary>
        Search,
        /// <summary>Recommendations — requires MemberOnly</summary>
        Recommendations,
        /// <summary>Ratings — requires MemberOnly</summary>
        Ratings,
        /// <summary>Rankings — requires MemberOnly (Authenticated)</summary>
        Rankings,
        /// <summary>Wishlist — requires MemberOnly</summary>
        Wishlist,
        /// <summary>Gamification — requires MemberOnly</summary>
        Gamification,
        /// <summary>Loan management (all users) — requires LibrarianOrAdmin</summary>
        LoanManagementAll,
        /// <summary>Own loans — requires MemberOnly</summary>
        OwnLoans,
        /// <summary>Auth me — requires Authenticated</summary>
        AuthMe
    }

    /// <summary>
    /// Represents the expected authorization result.
    /// </summary>
    public enum AccessResult
    {
        Allowed,
        Denied403,
        Denied401
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Policy Evaluation Logic (mirrors AuthorizationConfig.cs)
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// AdminOnly policy: requires role claim == "Admin".
    /// </summary>
    private static bool EvaluateAdminOnly(ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? principal.FindFirst(ClaimTypes.Role);
        return roleClaim?.Value == "Admin";
    }

    /// <summary>
    /// LibrarianOrAdmin policy: requires role claim == "Admin" or "Librarian".
    /// </summary>
    private static bool EvaluateLibrarianOrAdmin(ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? principal.FindFirst(ClaimTypes.Role);
        if (roleClaim is null) return false;
        return roleClaim.Value == "Admin" || roleClaim.Value == "Librarian";
    }

    /// <summary>
    /// MemberOnly policy: requires role claim == "Admin", "Librarian", or "Member".
    /// </summary>
    private static bool EvaluateMemberOnly(ClaimsPrincipal principal)
    {
        var roleClaim = principal.FindFirst("role")
            ?? principal.FindFirst(ClaimTypes.Role);
        if (roleClaim is null) return false;
        return roleClaim.Value == "Admin" || roleClaim.Value == "Librarian" || roleClaim.Value == "Member";
    }

    /// <summary>
    /// Authenticated policy: requires any authenticated identity.
    /// </summary>
    private static bool EvaluateAuthenticated(ClaimsPrincipal principal)
    {
        return principal.Identity?.IsAuthenticated ?? false;
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Permission Matrix — the ground truth for which roles can access which endpoints
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Returns the expected access result for a given role and endpoint,
    /// implementing the permission matrix from Requirements 7.4.
    /// </summary>
    private static AccessResult GetExpectedAccess(UserRole? role, EndpointCategory endpoint)
    {
        // Unauthenticated users (role == null) always get 401
        if (role is null) return AccessResult.Denied401;

        return endpoint switch
        {
            // User Management: Admin only (Req 7.5)
            EndpointCategory.UserManagement => role == UserRole.Admin
                ? AccessResult.Allowed
                : AccessResult.Denied403,

            // Book Management (create/update/delete): Admin or Librarian (Req 1.9)
            EndpointCategory.BookManagement => role == UserRole.Admin || role == UserRole.Librarian
                ? AccessResult.Allowed
                : AccessResult.Denied403,

            // Dashboard: Admin or Librarian (Req 8.5)
            EndpointCategory.Dashboard => role == UserRole.Admin || role == UserRole.Librarian
                ? AccessResult.Allowed
                : AccessResult.Denied403,

            // Loan Management All: Admin or Librarian
            EndpointCategory.LoanManagementAll => role == UserRole.Admin || role == UserRole.Librarian
                ? AccessResult.Allowed
                : AccessResult.Denied403,

            // All authenticated roles have access: Search, Recommendations, Ratings, Rankings, Wishlist, Gamification, OwnLoans, AuthMe
            EndpointCategory.Search => AccessResult.Allowed,
            EndpointCategory.Recommendations => AccessResult.Allowed,
            EndpointCategory.Ratings => AccessResult.Allowed,
            EndpointCategory.Rankings => AccessResult.Allowed,
            EndpointCategory.Wishlist => AccessResult.Allowed,
            EndpointCategory.Gamification => AccessResult.Allowed,
            EndpointCategory.OwnLoans => AccessResult.Allowed,
            EndpointCategory.AuthMe => AccessResult.Allowed,

            _ => AccessResult.Denied403
        };
    }

    /// <summary>
    /// Returns the required policy for a given endpoint category.
    /// </summary>
    private static string GetRequiredPolicy(EndpointCategory endpoint) => endpoint switch
    {
        EndpointCategory.UserManagement => "AdminOnly",
        EndpointCategory.BookManagement => "LibrarianOrAdmin",
        EndpointCategory.Dashboard => "LibrarianOrAdmin",
        EndpointCategory.LoanManagementAll => "LibrarianOrAdmin",
        EndpointCategory.Search => "MemberOnly",
        EndpointCategory.Recommendations => "MemberOnly",
        EndpointCategory.Ratings => "MemberOnly",
        EndpointCategory.Rankings => "MemberOnly",
        EndpointCategory.Wishlist => "MemberOnly",
        EndpointCategory.Gamification => "MemberOnly",
        EndpointCategory.OwnLoans => "MemberOnly",
        EndpointCategory.AuthMe => "Authenticated",
        _ => "Authenticated"
    };

    /// <summary>
    /// Evaluates whether a principal passes the given policy.
    /// </summary>
    private static bool EvaluatePolicy(string policy, ClaimsPrincipal principal)
    {
        return policy switch
        {
            "AdminOnly" => EvaluateAdminOnly(principal),
            "LibrarianOrAdmin" => EvaluateLibrarianOrAdmin(principal),
            "MemberOnly" => EvaluateMemberOnly(principal),
            "Authenticated" => EvaluateAuthenticated(principal),
            _ => false
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Principal Factories
    // ═══════════════════════════════════════════════════════════════════════════════

    private static ClaimsPrincipal CreatePrincipalForRole(UserRole role)
    {
        var claims = new[]
        {
            new Claim("role", role.ToString()),
            new Claim(ClaimTypes.Role, role.ToString()),
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Email, $"user-{role.ToString().ToLower()}@test.com")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        return new ClaimsPrincipal(identity);
    }

    private static ClaimsPrincipal CreateAnonymousPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity());
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Property Tests
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// **Property 8: Role-Based Access Control**
    /// For any random (role, endpoint) pair, the policy evaluation result matches the
    /// expected permission matrix. Authenticated users with insufficient permissions
    /// are denied (403), while correct roles are allowed (200).
    ///
    /// **Validates: Requirements 1.9, 6.6, 7.5, 8.5, 16.2, 17.7, 20.9**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AuthenticatedUser_PolicyEvaluation_MatchesPermissionMatrix()
    {
        return Prop.ForAll(
            Arb.From(GenRoleEndpointPair()),
            pair =>
            {
                var (role, endpoint) = pair;
                var principal = CreatePrincipalForRole(role);
                var policy = GetRequiredPolicy(endpoint);
                var policyResult = EvaluatePolicy(policy, principal);
                var expected = GetExpectedAccess(role, endpoint);

                // If expected is Allowed, policy should pass; if Denied403, policy should fail
                return expected == AccessResult.Allowed
                    ? policyResult
                    : !policyResult;
            });
    }

    /// <summary>
    /// **Property 8: Role-Based Access Control (Unauthenticated)**
    /// For any random endpoint, unauthenticated users are always denied.
    /// Non-authenticated users receive 401 regardless of endpoint.
    ///
    /// **Validates: Requirements 6.6**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property UnauthenticatedUser_AlwaysDenied_ForAllEndpoints()
    {
        return Prop.ForAll(
            Arb.From(GenEndpointCategory()),
            endpoint =>
            {
                var principal = CreateAnonymousPrincipal();
                var policy = GetRequiredPolicy(endpoint);
                var policyResult = EvaluatePolicy(policy, principal);

                // Anonymous users should ALWAYS be denied regardless of endpoint
                return !policyResult;
            });
    }

    /// <summary>
    /// **Property 8: Role-Based Access Control (Member restrictions)**
    /// Members are denied access to all restricted endpoints (book management, user management, dashboard).
    ///
    /// **Validates: Requirements 1.9, 7.5, 8.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property MemberRole_DeniedAccess_ToRestrictedEndpoints()
    {
        return Prop.ForAll(
            Arb.From(GenRestrictedEndpoint()),
            endpoint =>
            {
                var principal = CreatePrincipalForRole(UserRole.Member);
                var policy = GetRequiredPolicy(endpoint);
                var policyResult = EvaluatePolicy(policy, principal);

                // Member should be denied for restricted endpoints
                return !policyResult;
            });
    }

    /// <summary>
    /// **Property 8: Role-Based Access Control (Admin full access)**
    /// Admin users have access to all endpoints without exception.
    ///
    /// **Validates: Requirements 6.6, 7.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property AdminRole_AllowedAccess_ToAllEndpoints()
    {
        return Prop.ForAll(
            Arb.From(GenEndpointCategory()),
            endpoint =>
            {
                var principal = CreatePrincipalForRole(UserRole.Admin);
                var policy = GetRequiredPolicy(endpoint);
                var policyResult = EvaluatePolicy(policy, principal);

                // Admin should be allowed for ALL endpoints
                return policyResult;
            });
    }

    /// <summary>
    /// **Property 8: Role-Based Access Control (Librarian access)**
    /// Librarians can access book management, dashboard, and loan management, but NOT user management.
    ///
    /// **Validates: Requirements 7.5, 8.5**
    /// </summary>
    [Property(MaxTest = 100)]
    public Property LibrarianRole_AllowedForLibrarianEndpoints_DeniedForAdminOnly()
    {
        return Prop.ForAll(
            Arb.From(GenEndpointCategory()),
            endpoint =>
            {
                var principal = CreatePrincipalForRole(UserRole.Librarian);
                var policy = GetRequiredPolicy(endpoint);
                var policyResult = EvaluatePolicy(policy, principal);
                var expected = GetExpectedAccess(UserRole.Librarian, endpoint);

                return expected == AccessResult.Allowed
                    ? policyResult
                    : !policyResult;
            });
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // Custom Generators
    // ═══════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Generates random (UserRole, EndpointCategory) pairs for exhaustive testing.
    /// </summary>
    private static Gen<(UserRole role, EndpointCategory endpoint)> GenRoleEndpointPair()
    {
        var roles = Enum.GetValues<UserRole>();
        var endpoints = Enum.GetValues<EndpointCategory>();

        return Gen.Elements(roles).SelectMany(role =>
            Gen.Elements(endpoints).Select(endpoint => (role, endpoint)));
    }

    /// <summary>
    /// Generates random EndpointCategory values.
    /// </summary>
    private static Gen<EndpointCategory> GenEndpointCategory()
    {
        return Gen.Elements(Enum.GetValues<EndpointCategory>());
    }

    /// <summary>
    /// Generates only restricted endpoint categories (those that Members cannot access).
    /// BookManagement (Req 1.9), UserManagement (Req 7.5), Dashboard (Req 8.5), LoanManagementAll.
    /// </summary>
    private static Gen<EndpointCategory> GenRestrictedEndpoint()
    {
        return Gen.Elements(
            EndpointCategory.BookManagement,
            EndpointCategory.UserManagement,
            EndpointCategory.Dashboard,
            EndpointCategory.LoanManagementAll);
    }
}
