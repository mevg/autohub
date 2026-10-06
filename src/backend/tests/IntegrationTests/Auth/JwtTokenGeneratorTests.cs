using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoHub.Domain.Entities;
using AutoHub.Domain.Enums;
using AutoHub.Infrastructure.Authentication;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;

namespace AutoHub.IntegrationTests.Auth;

public class JwtTokenGeneratorTests
{
    private readonly JwtSettings _settings = new()
    {
        Secret = "Integration_Tests_Secret_Must_Be_Long_Enough_32B+",
        Issuer = "AutoHubAPI",
        Audience = "AutoHubClient",
        ExpiryHours = 8
    };

    [Fact]
    public void GenerateToken_produces_validatable_token_with_expected_claims()
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@autohub.local",
            FullName = "Administrator",
            Role = UserRole.Admin
        };

        var token = new JwtTokenGenerator(_settings).GenerateToken(user);

        var principal = new JwtSecurityTokenHandler().ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _settings.Issuer,
            ValidAudience = _settings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_settings.Secret)),
            ClockSkew = TimeSpan.Zero
        }, out _);

        // Al validar, el handler remapea "sub" -> NameIdentifier y "email" -> ClaimTypes.Email.
        principal.FindFirstValue(ClaimTypes.NameIdentifier).Should().Be(user.Id.ToString());
        principal.FindFirstValue(ClaimTypes.Email).Should().Be(user.Email);
        principal.FindFirstValue(ClaimTypes.Role).Should().Be("Admin");
        principal.FindFirstValue("fullName").Should().Be("Administrator");
    }
}
