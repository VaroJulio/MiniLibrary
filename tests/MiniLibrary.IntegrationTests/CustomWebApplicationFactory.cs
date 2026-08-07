using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MiniLibrary.Infrastructure.Data;

namespace MiniLibrary.IntegrationTests;

/// <summary>
/// Custom WebApplicationFactory that uses an in-memory database for integration tests.
/// Avoids requiring a real SQL Server/TestContainers for fast CI feedback.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private const string TestJwtSecret = "IntegrationTestSecretKeyThatIsLongEnoughForHS256!";
    private const string TestJwtIssuer = "MiniLibrary";
    private const string TestJwtAudience = "MiniLibrary";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Prevent JWT claim type mapping (role → ClaimTypes.Role URI)
        JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the real DbContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            // Remove interceptors that depend on MediatR (avoid EF triggering domain events in tests)
            var interceptorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DomainEventDispatcher));
            if (interceptorDescriptor != null) services.Remove(interceptorDescriptor);
            services.AddScoped<DomainEventDispatcher>();

            // Use in-memory database for tests
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseInMemoryDatabase("MiniLibraryTestDb");
            });
        });

        builder.UseSetting("Jwt:Secret", TestJwtSecret);
        builder.UseSetting("Jwt:Issuer", TestJwtIssuer);
        builder.UseSetting("Jwt:Audience", TestJwtAudience);
        builder.UseSetting("Authentication:Google:ClientId", "test-google-client");
        builder.UseSetting("Authentication:Google:ClientSecret", "test-google-secret");
        builder.UseSetting("Authentication:Microsoft:ClientId", "test-ms-client");
        builder.UseSetting("Authentication:Microsoft:ClientSecret", "test-ms-secret");
    }

    /// <summary>
    /// Generates a JWT token for testing with the specified role and user info.
    /// </summary>
    public string GenerateTestToken(string userId = "test-user-001", string email = "test@example.com",
        string name = "Test User", string role = "Member")
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestJwtSecret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Name, name),
            new Claim(ClaimTypes.Role, role),
            new Claim("role", role),
        };

        var token = new JwtSecurityToken(
            issuer: TestJwtIssuer,
            audience: TestJwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Creates an HttpClient with an Authorization header set to the given role.
    /// </summary>
    public HttpClient CreateAuthenticatedClient(string role = "Member", string userId = "test-user-001")
    {
        var client = CreateClient();
        var token = GenerateTestToken(userId: userId, role: role);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }
}
