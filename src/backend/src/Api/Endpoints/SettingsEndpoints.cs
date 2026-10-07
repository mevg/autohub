using AutoHub.Application.Features.Settings.Commands.RemoveBanner;
using AutoHub.Application.Features.Settings.Commands.UpdateBranding;
using AutoHub.Application.Features.Settings.Commands.UploadBrandAsset;
using AutoHub.Application.Features.Settings.Queries.GetBranding;
using FluentValidation;
using MediatR;

namespace AutoHub.Api.Endpoints;

public static class SettingsEndpoints
{
    public static IEndpointRouteBuilder MapSettingsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/settings").WithTags("Settings");

        group.MapGet("/branding", async (ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetBrandingQuery(), ct)))
        .WithName("GetBranding")
        .AllowAnonymous();

        group.MapPut("/branding", async (UpdateBrandingCommand command, ISender sender, CancellationToken ct) =>
        {
            try
            {
                await sender.Send(command, ct);
                return Results.Ok(await sender.Send(new GetBrandingQuery(), ct));
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UpdateBranding")
        .RequireAuthorization("Admin");

        group.MapPost("/branding/assets/{slot}", async (
            string slot,
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
                    new UploadBrandAssetCommand(slot, file.ContentType, buffer.ToArray()), ct);
                return Results.Ok(result);
            }
            catch (ValidationException ex)
            {
                return Results.ValidationProblem(ToErrors(ex));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("UploadBrandAsset")
        .RequireAuthorization("Admin")
        .DisableAntiforgery();

        group.MapDelete("/branding/banners/{index:int}", async (
            int index,
            ISender sender,
            CancellationToken ct) =>
        {
            try
            {
                return Results.Ok(await sender.Send(new RemoveBannerCommand(index), ct));
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        })
        .WithName("RemoveBanner")
        .RequireAuthorization("Admin");

        return app;
    }

    private static Dictionary<string, string[]> ToErrors(ValidationException ex)
        => ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
}
