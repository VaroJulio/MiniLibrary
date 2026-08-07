using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using MiniLibrary.Domain.Entities;
using MiniLibrary.Domain.Enumerations;
using MiniLibrary.Infrastructure.Services;

namespace MiniLibrary.UnitTests.Services;

public class JwtTokenServiceTests
{
    private readonly JwtTokenService _sut;
    private readonly IConfiguration _configuration;

    public JwtTokenServiceTests()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["Jwt:Secret"] = "MiniLibrary-Test-Secret-Key-That-Is-Long-Enough-For-HmacSha256!",
            ["Jwt:Issuer"] = "MiniLibrary",
            ["Jwt:Audience"] = "MiniLibrary",
            ["Jwt:ExpirationMinutes"] = "60",
            ["Jwt:RefreshExpirationDays"] = "7"
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        _sut = new JwtTokenService(_configuration);
    }

    [Fact]
    public void GenerateAccessToken_ReturnsValidJwt_WithCorrectClaims()
    {
        // Arrange
        var user = User.Create(
            email: "test@example.com",
            fullName: "Test User",
            externalId: "ext-123",
            provider: "Google",
            role: UserRole.Member);

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeNullOrEmpty();

        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Issuer.Should().Be("MiniLibrary");
        jwt.Audiences.Should().Contain("MiniLibrary");
        jwt.Claims.Should().Contain(c => c.Type == "userId" && c.Value == user.Id.ToString());
        jwt.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "test@example.com");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Member");
    }

    [Fact]
    public void GenerateAccessToken_HasSixtyMinuteExpiration()
    {
        // Arrange
        var user = User.Create(
            email: "test@example.com",
            fullName: "Test User",
            externalId: "ext-123",
            provider: "Google",
            role: UserRole.Member);

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var expectedExpiry = beforeGeneration.AddMinutes(60);
        jwt.ValidTo.Should().BeCloseTo(expectedExpiry, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsUniqueBase64String()
    {
        // Act
        var token1 = _sut.GenerateRefreshToken();
        var token2 = _sut.GenerateRefreshToken();

        // Assert
        token1.Should().NotBeNullOrEmpty();
        token2.Should().NotBeNullOrEmpty();
        token1.Should().NotBe(token2);

        // Should be valid base64
        var bytes = Convert.FromBase64String(token1);
        bytes.Length.Should().Be(64);
    }

    [Fact]
    public void StoreRefreshToken_And_ValidateRefreshToken_ReturnsUserId()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = _sut.GenerateRefreshToken();

        // Act
        _sut.StoreRefreshToken(userId, refreshToken);
        var result = _sut.ValidateRefreshToken(refreshToken);

        // Assert
        result.Should().Be(userId);
    }

    [Fact]
    public void ValidateRefreshToken_ReturnsNull_ForUnknownToken()
    {
        // Act
        var result = _sut.ValidateRefreshToken("unknown-token");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void StoreRefreshToken_ReplacesExistingTokenForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var oldToken = _sut.GenerateRefreshToken();
        var newToken = _sut.GenerateRefreshToken();

        _sut.StoreRefreshToken(userId, oldToken);

        // Act
        _sut.StoreRefreshToken(userId, newToken);

        // Assert
        _sut.ValidateRefreshToken(oldToken).Should().BeNull();
        _sut.ValidateRefreshToken(newToken).Should().Be(userId);
    }

    [Fact]
    public void RevokeRefreshToken_RemovesAllTokensForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = _sut.GenerateRefreshToken();
        _sut.StoreRefreshToken(userId, refreshToken);

        // Act
        _sut.RevokeRefreshToken(userId);

        // Assert
        _sut.ValidateRefreshToken(refreshToken).Should().BeNull();
    }

    [Fact]
    public void GenerateAccessToken_IncludesCorrectRoleForAdmin()
    {
        // Arrange
        var user = User.Create(
            email: "admin@example.com",
            fullName: "Admin User",
            externalId: "ext-admin",
            provider: "Microsoft",
            role: UserRole.Admin);

        // Act
        var token = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "Admin");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "Admin");
    }

    [Fact]
    public void GenerateAccessToken_IncludesUniqueJti()
    {
        // Arrange
        var user = User.Create(
            email: "test@example.com",
            fullName: "Test User",
            externalId: "ext-123",
            provider: "Google",
            role: UserRole.Member);

        // Act
        var token1 = _sut.GenerateAccessToken(user);
        var token2 = _sut.GenerateAccessToken(user);

        // Assert
        var handler = new JwtSecurityTokenHandler();
        var jwt1 = handler.ReadJwtToken(token1);
        var jwt2 = handler.ReadJwtToken(token2);

        var jti1 = jwt1.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;
        var jti2 = jwt2.Claims.First(c => c.Type == JwtRegisteredClaimNames.Jti).Value;

        jti1.Should().NotBe(jti2);
    }
}
