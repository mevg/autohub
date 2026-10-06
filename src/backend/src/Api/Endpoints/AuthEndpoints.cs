using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoHub.Application.Features.Auth.Commands.Login;
using FluentValidation;
using MediatR;

namespace AutoHub.Api.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
        {
            try
            {
                var result = await sender.Send(command, ct);
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                var errors = ex.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                return Results.ValidationProblem(errors);
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Unauthorized();
            }
        })
        .WithName("Login")
        .AllowAnonymous();

        group.MapGet("/me", (ClaimsPrincipal user) => Results.Ok(new
        {
            // Al validar el token, JwtSecurityTokenHandler remapea "sub" -> NameIdentifier
            // y "email" -> ClaimTypes.Email (DefaultMapInboundClaims). Se lee con fallback.
            Id = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? user.FindFirstValue(ClaimTypes.NameIdentifier),
            Email = user.FindFirstValue(JwtRegisteredClaimNames.Email)
                ?? user.FindFirstValue(ClaimTypes.Email),
            FullName = user.FindFirstValue("fullName"),
            Role = user.FindFirstValue(ClaimTypes.Role)
        }))
        .WithName("GetCurrentUser")
        .RequireAuthorization();

        return app;
    }
}
