using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AutoHub.Application.Common.Exceptions;
using AutoHub.Application.Features.Vehicles.Commands.ArchiveVehicle;
using AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;
using AutoHub.Application.Features.Vehicles.Commands.RemoveVehicleImage;
using AutoHub.Application.Features.Vehicles.Commands.UpdateVehicle;
using AutoHub.Application.Features.Vehicles.Commands.UploadVehicleImage;
using AutoHub.Application.Features.Vehicles.Queries.GetVehicleById;
using AutoHub.Application.Features.Vehicles.Queries.ListVehicles;
using FluentValidation;
using MediatR;

namespace AutoHub.Api.Endpoints;

public static class VehicleEndpoints
{
    public static IEndpointRouteBuilder MapVehicleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/vehicles").WithTags("Vehicles").RequireAuthorization();

        group.MapGet("/", async (
            ISender sender,
            CancellationToken ct,
            string? search = null,
            string? status = null,
            int? yearFrom = null,
            int? yearTo = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            int page = 1,
            int pageSize = 20) =>
        {
            var filters = new VehicleFilters(search, status, yearFrom, yearTo, priceFrom, priceTo, page, pageSize);
            return Results.Ok(await sender.Send(new ListVehiclesQuery(filters), ct));
        })
        .WithName("ListVehicles");

        group.MapGet("/{id:guid}", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await sender.Send(new GetVehicleByIdQuery(id), ct));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("GetVehicleById");

        group.MapPost("/", async (
            CreateVehicleCommand command,
            ClaimsPrincipal user,
            ISender sender,
            CancellationToken ct) =>
        {
            try
            {
                var createdBy = GetUserId(user);
                var id = await sender.Send(command with { CreatedBy = createdBy }, ct);
                return Results.Created($"/api/vehicles/{id}", new { id });
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToErrors(ex));
            }
            catch (DuplicateVinException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
        })
        .WithName("CreateVehicle");

        group.MapPut("/{id:guid}", async (
            Guid id,
            UpdateVehicleCommand command,
            ISender sender,
            CancellationToken ct) =>
        {
            try
            {
                await sender.Send(command with { Id = id }, ct);
                return Results.Ok(await sender.Send(new GetVehicleByIdQuery(id), ct));
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToErrors(ex));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (DuplicateVinException ex)
            {
                return Results.Conflict(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UpdateVehicle");

        group.MapPost("/{id:guid}/archive", async (Guid id, ISender sender, CancellationToken ct) =>
        {
            try
            {
                await sender.Send(new ArchiveVehicleCommand(id), ct);
                return Results.Ok(await sender.Send(new GetVehicleByIdQuery(id), ct));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("ArchiveVehicle");

        group.MapPost("/{id:guid}/images", async (
            Guid id,
            IFormFile file,
            ISender sender,
            CancellationToken ct) =>
        {
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { message = "Archivo requerido." });
            }

            await using var stream = file.OpenReadStream();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer, ct);

            try
            {
                var result = await sender.Send(
                    new UploadVehicleImageCommand(id, file.ContentType, buffer.ToArray()), ct);
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToErrors(ex));
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UploadVehicleImage")
        .DisableAntiforgery();

        group.MapDelete("/{id:guid}/images/{imageId:guid}", async (
            Guid id,
            Guid imageId,
            ISender sender,
            CancellationToken ct) =>
        {
            try
            {
                await sender.Send(new RemoveVehicleImageCommand(id, imageId), ct);
                return Results.NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
        })
        .WithName("RemoveVehicleImage");

        return app;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var sub = user.FindFirstValue(JwtRegisteredClaimNames.Sub)
            ?? user.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(sub, out var id) ? id : Guid.Empty;
    }

    private static Dictionary<string, string[]> ToErrors(ValidationException ex)
        => ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
