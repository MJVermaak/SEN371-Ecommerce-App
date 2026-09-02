using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Infrastructure.Security;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GrandmastersHub.Tests;

public sealed class JwtTokenServiceTests
{
    private const string Issuer = "GrandmastersHub.Tests";
    private const string Audience = "GrandmastersHub.Tests.Client";
    private const string SigningKey = "test-only-signing-key-with-at-least-32-bytes";

    [Fact]
    public async Task CreateToken_ProducesAValidSignedTokenWithIdentityAndRoleClaims()
    {
        var user = new User { UserId = 42, Email = "admin@example.com", PasswordHash = "not-used", Role = "Admin", CreatedAt = DateTime.UtcNow };
        var service = CreateService(SigningKey);
        var response = service.CreateToken(user);
        var principal = await new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateTokenAsync(response.AccessToken, ValidationParameters(SigningKey));

        Assert.True(principal.IsValid);
        Assert.Equal(user.UserId.ToString(), principal.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.Equal(user.Email, principal.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Email)?.Value);
        Assert.Equal(user.Role, principal.ClaimsIdentity.FindFirst(ClaimTypes.Role)?.Value);
        Assert.NotNull(principal.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti));
        Assert.Equal(user.UserId, response.UserId);
        Assert.Equal(user.Role, response.Role);
        Assert.InRange(response.ExpiresAtUtc, DateTimeOffset.UtcNow.AddMinutes(14), DateTimeOffset.UtcNow.AddMinutes(16));
    }

    [Fact]
    public async Task CreateToken_CannotBeValidatedWithADifferentSigningKey()
    {
        var user = new User { UserId = 7, Email = "player@example.com", PasswordHash = "not-used", Role = "Customer", CreatedAt = DateTime.UtcNow };
        var response = CreateService(SigningKey).CreateToken(user);
        var result = await new JwtSecurityTokenHandler().ValidateTokenAsync(response.AccessToken, ValidationParameters("a-different-test-key-that-is-also-long-enough"));
        Assert.False(result.IsValid);
    }

    private static JwtTokenService CreateService(string key) => new(Options.Create(new JwtOptions { Issuer = Issuer, Audience = Audience, SigningKey = key, ExpiryMinutes = 15 }));
    private static TokenValidationParameters ValidationParameters(string key) => new()
    {
        ValidateIssuer = true, ValidIssuer = Issuer, ValidateAudience = true, ValidAudience = Audience,
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateLifetime = true, ClockSkew = TimeSpan.Zero
    };
}
