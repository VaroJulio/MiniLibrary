using Microsoft.AspNetCore.Authorization;

namespace MiniLibrary.API.Configuration;

/// <summary>
/// Configures role-based authorization policies for the application.
/// </summary>
public static class AuthorizationConfig
{
    /// <summary>
    /// Policy names used throughout the application with [Authorize(Policy = "...")] attributes.
    /// </summary>
    public static class Policies
    {
        /// <summary>Requires the Admin role.</summary>
        public const string AdminOnly = "AdminOnly";

        /// <summary>Requires Admin or Librarian role.</summary>
        public const string LibrarianOrAdmin = "LibrarianOrAdmin";

        /// <summary>Requires any authenticated user (Member or higher).</summary>
        public const string MemberOnly = "MemberOnly";

        /// <summary>Any authenticated user.</summary>
        public const string Authenticated = "Authenticated";
    }

    /// <summary>
    /// Registers authorization policies based on the permission matrix defined in Requirements 7.4.
    /// </summary>
    /// <remarks>
    /// Permission matrix:
    /// | Resource          | Admin | Librarian | Member       |
    /// |-------------------|-------|-----------|--------------|
    /// | Book CRUD         | Yes   | Yes       | Read only    |
    /// | Loan Management   | Yes   | Yes (all) | Own only     |
    /// | User Management   | Yes   | No        | No           |
    /// | Dashboard         | Yes   | Yes       | No           |
    /// | Search            | Yes   | Yes       | Yes          |
    /// | Recommendations   | Yes   | Yes       | Yes          |
    /// | Ratings           | Yes   | Yes       | Yes (own)    |
    /// | Rankings          | Yes   | Yes       | Yes          |
    /// | Wishlist          | Yes   | Yes       | Yes (own)    |
    /// | Gamification      | Yes   | Yes       | Yes          |
    /// </remarks>
    public static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // AdminOnly — requires Role == Admin
            options.AddPolicy(Policies.AdminOnly, policy =>
                policy.RequireClaim("role", "Admin"));

            // LibrarianOrAdmin — requires Role == Admin or Librarian
            options.AddPolicy(Policies.LibrarianOrAdmin, policy =>
                policy.RequireAssertion(context =>
                {
                    var roleClaim = context.User.FindFirst("role")
                        ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                    if (roleClaim is null) return false;
                    return roleClaim.Value == "Admin" || roleClaim.Value == "Librarian";
                }));

            // MemberOnly — any authenticated user (Member, Librarian, or Admin)
            options.AddPolicy(Policies.MemberOnly, policy =>
                policy.RequireAssertion(context =>
                {
                    var roleClaim = context.User.FindFirst("role")
                        ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.Role);
                    if (roleClaim is null) return false;
                    return roleClaim.Value == "Admin"
                        || roleClaim.Value == "Librarian"
                        || roleClaim.Value == "Member";
                }));

            // Authenticated — any authenticated user regardless of role
            options.AddPolicy(Policies.Authenticated, policy =>
                policy.RequireAuthenticatedUser());
        });

        return services;
    }
}
