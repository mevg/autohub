using AutoHub.Application.Common.Interfaces;
using AutoHub.Application.Features.PublicCatalog;
using AutoHub.Application.Features.PublicCatalog.Dtos;

namespace AutoHub.Api.Endpoints;

public static class PublicCatalogEndpoints
{
    public static IEndpointRouteBuilder MapPublicCatalogEndpoints(
        this IEndpointRouteBuilder app,
        WhatsAppSettings whatsAppSettings)
    {
        var group = app.MapGroup("/api/public")
            .WithTags("Public Catalog")
            .AllowAnonymous();

        group.MapGet("/vehicles", async (
            IPublicCatalogQueries queries,
            CancellationToken ct,
            string? search = null,
            string? make = null,
            string? model = null,
            int? yearFrom = null,
            int? yearTo = null,
            decimal? priceFrom = null,
            decimal? priceTo = null,
            string? sort = null,
            int page = 1,
            int pageSize = 12) =>
        {
            var filters = new PublicCatalogFilters(
                search, make, model, yearFrom, yearTo, priceFrom, priceTo, sort, page, pageSize);

            var errors = Validate(filters);
            if (errors.Count > 0)
            {
                return Results.ValidationProblem(errors);
            }

            return Results.Ok(await queries.ListAsync(filters, ct));
        })
        .WithName("ListPublicVehicles");

        group.MapGet("/vehicles/filters", async (
            IPublicCatalogQueries queries,
            CancellationToken ct,
            string? make = null) =>
        {
            if (make is { Length: > PublicCatalogFilters.MaxFacetValueLength })
            {
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["make"] = [$"La marca no puede superar {PublicCatalogFilters.MaxFacetValueLength} caracteres."]
                });
            }

            return Results.Ok(await queries.GetFacetsAsync(make, ct));
        })
        .WithName("GetPublicVehicleFilters");

        group.MapGet("/vehicles/{id:guid}", async (
            Guid id,
            IPublicCatalogQueries queries,
            CancellationToken ct) =>
        {
            var vehicle = await queries.GetByIdAsync(id, ct);
            if (vehicle is null)
            {
                return Results.NotFound(new { message = "Vehículo no encontrado." });
            }

            var whatsAppUrl = WhatsAppLinkBuilder.Build(
                whatsAppSettings,
                vehicle.Make,
                vehicle.Model,
                vehicle.Year,
                vehicle.PublicReference);

            return Results.Ok(vehicle with { WhatsAppUrl = whatsAppUrl });
        })
        .WithName("GetPublicVehicle");

        return app;
    }

    private static Dictionary<string, string[]> Validate(PublicCatalogFilters filters)
    {
        var errors = new Dictionary<string, string[]>();
        var currentYear = DateTime.UtcNow.Year + 1;

        if (filters.Search is { Length: > PublicCatalogFilters.MaxSearchLength })
        {
            errors["search"] = [$"La búsqueda no puede superar {PublicCatalogFilters.MaxSearchLength} caracteres."];
        }

        if (filters.Make is { Length: > PublicCatalogFilters.MaxFacetValueLength })
        {
            errors["make"] = [$"La marca no puede superar {PublicCatalogFilters.MaxFacetValueLength} caracteres."];
        }

        if (filters.Model is { Length: > PublicCatalogFilters.MaxFacetValueLength })
        {
            errors["model"] = [$"El modelo no puede superar {PublicCatalogFilters.MaxFacetValueLength} caracteres."];
        }

        if (filters.YearFrom is < 1900 or > 9999 || filters.YearTo is < 1900 or > 9999)
        {
            errors["year"] = ["El rango de años es inválido."];
        }
        else if (filters.YearFrom > filters.YearTo)
        {
            errors["year"] = ["El año inicial no puede ser mayor que el año final."];
        }
        else if (filters.YearFrom > currentYear || filters.YearTo > currentYear)
        {
            errors["year"] = [$"El año no puede ser mayor que {currentYear}."];
        }

        if (filters.PriceFrom is < 0 || filters.PriceTo is < 0)
        {
            errors["price"] = ["El precio no puede ser negativo."];
        }
        else if (filters.PriceFrom > filters.PriceTo)
        {
            errors["price"] = ["El precio mínimo no puede ser mayor que el máximo."];
        }

        return errors;
    }
}
