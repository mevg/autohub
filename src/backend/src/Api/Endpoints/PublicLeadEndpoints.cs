using AutoHub.Application.Common.Exceptions;
using AutoHub.Application.Common.Security;
using AutoHub.Application.Features.Leads.CreateLead;
using FluentValidation;
using MediatR;

namespace AutoHub.Api.Endpoints;

public static class PublicLeadEndpoints
{
    public static IEndpointRouteBuilder MapPublicLeadEndpoints(
        this IEndpointRouteBuilder app,
        string? ipHashSalt)
    {
        var group = app.MapGroup("/api/public/vehicles/{vehicleId:guid}/leads")
            .WithTags("Public Leads")
            .AllowAnonymous()
            .RequireRateLimiting("public-leads");

        group.MapPost("/", async (
            Guid vehicleId,
            CreateLeadRequest request,
            HttpContext http,
            ISender sender,
            ILoggerFactory loggerFactory,
            CancellationToken ct) =>
        {
            var logger = loggerFactory.CreateLogger("PublicLeads");

            // Honeypot: respondemos como éxito para no dar pistas al bot, sin persistir.
            if (!string.IsNullOrWhiteSpace(request.Website))
            {
                logger.LogWarning("Lead descartado por honeypot para el vehículo {VehicleId}.", vehicleId);
                var fakeId = Guid.NewGuid();
                return Results.Created($"/api/public/leads/{fakeId}", new { id = fakeId });
            }

            var idempotencyKey = http.Request.Headers["Idempotency-Key"].FirstOrDefault();
            var command = new CreateLeadCommand(
                vehicleId,
                request.Name ?? string.Empty,
                request.Phone ?? string.Empty,
                request.Email ?? string.Empty,
                request.Message,
                request.Website,
                idempotencyKey,
                request.ConsentAccepted,
                IpHash.Create(http.Connection.RemoteIpAddress?.ToString(), ipHashSalt),
                http.Request.Headers.UserAgent.ToString());

            try
            {
                var result = await sender.Send(command, ct);
                var payload = new { id = result.Id };

                return result.Created
                    ? Results.Created($"/api/public/leads/{result.Id}", payload)
                    : Results.Ok(payload);
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToErrors(ex));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (IdempotencyConflictException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .WithName("CreatePublicLead");

        return app;
    }

    private static Dictionary<string, string[]> ToErrors(ValidationException ex)
        => ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}

public sealed record CreateLeadRequest(
    string? Name,
    string? Phone,
    string? Email,
    string? Message,
    bool ConsentAccepted = false,
    string? Website = null);
