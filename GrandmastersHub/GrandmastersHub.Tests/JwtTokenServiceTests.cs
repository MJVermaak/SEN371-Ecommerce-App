using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GrandmastersHub.Domain.Entities;
using GrandmastersHub.Domain.Enums;
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
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@example.com",
            NormalizedEmail = "ADMIN@EXAMPLE.COM",
            PasswordHash = "not-used",
            Role = UserRole.Admin,
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            SigningKey = SigningKey,
            ExpiryMinutes = 15
        }));

        var response = service.CreateToken(user);
        var handler = new JwtSecurityTokenHandler { MapInboundClaims = false };
        var principal = await handler.ValidateTokenAsync(
            response.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero
            });

        Assert.True(principal.IsValid);
        Assert.Equal(user.Id.ToString(), principal.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
        Assert.Equal(user.Email, principal.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Email)?.Value);
        Assert.Equal(UserRole.Admin.ToString(), principal.ClaimsIdentity.FindFirst(ClaimTypes.Role)?.Value);
        Assert.NotNull(principal.ClaimsIdentity.FindFirst(JwtRegisteredClaimNames.Jti));
        Assert.Equal(user.Id, response.UserId);
        Assert.Equal(UserRole.Admin.ToString(), response.Role);
        Assert.InRange(response.ExpiresAtUtc, DateTimeOffset.UtcNow.AddMinutes(14), DateTimeOffset.UtcNow.AddMinutes(16));
    }

    [Fact]
    public async Task CreateToken_CannotBeValidatedWithADifferentSigningKey()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "player@example.com",
            NormalizedEmail = "PLAYER@EXAMPLE.COM",
            PasswordHash = "not-used",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        var service = new JwtTokenService(Options.Create(new JwtOptions
        {
            Issuer = Issuer,
            Audience = Audience,
            SigningKey = SigningKey
        }));
        var response = service.CreateToken(user);

        var result = await new JwtSecurityTokenHandler().ValidateTokenAsync(
            response.AccessToken,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = Issuer,
                ValidateAudience = true,
                ValidAudience = Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("a-different-test-key-that-is-also-long-enough")),
                ValidateLifetime = true
            });

        Assert.False(result.IsValid);
    }
}
