using System.Text;
using AutoHub.Application.Common.Dtos;
using AutoHub.Application.Features.PublicCatalog;
using AutoHub.Application.Features.PublicCatalog.Dtos;
using AutoHub.Domain.Enums;

namespace AutoHub.Infrastructure.Persistence.Queries;

public sealed class DapperPublicCatalogQueries(ISqlConnectionFactory connectionFactory) : IPublicCatalogQueries
{
    public async Task<PagedResult<PublicVehicleCardDto>> ListAsync(
        PublicCatalogFilters filters,
        CancellationToken cancellationToken = default)
    {
        var page = filters.SafePage;
        var pageSize = filters.SafePageSize;

        var where = new StringBuilder(@"WHERE v.""Status"" = @Status");
        var parameters = new DynamicParameters();
        parameters.Add("Status", (int)VehicleStatus.Available);
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);

        if (filters.SafeSearch is { } search)
        {
            where.Append(@" AND (v.""Make"" ILIKE @Search OR v.""Model"" ILIKE @Search
                OR v.""PublicReference"" ILIKE @Search OR v.""Description"" ILIKE @Search)");
            parameters.Add("Search", $"%{search}%");
        }

        if (filters.SafeMake is { } make)
        {
            where.Append(@" AND v.""Make"" ILIKE @Make");
            parameters.Add("Make", make);
        }

        if (filters.SafeModel is { } model)
        {
            where.Append(@" AND v.""Model"" ILIKE @Model");
            parameters.Add("Model", model);
        }

        if (filters.YearFrom.HasValue)
        {
            where.Append(@" AND v.""Year"" >= @YearFrom");
            parameters.Add("YearFrom", filters.YearFrom.Value);
        }

        if (filters.YearTo.HasValue)
        {
            where.Append(@" AND v.""Year"" <= @YearTo");
            parameters.Add("YearTo", filters.YearTo.Value);
        }

        if (filters.PriceFrom.HasValue)
        {
            where.Append(@" AND v.""Price"" >= @PriceFrom");
            parameters.Add("PriceFrom", filters.PriceFrom.Value);
        }

        if (filters.PriceTo.HasValue)
        {
            where.Append(@" AND v.""Price"" <= @PriceTo");
            parameters.Add("PriceTo", filters.PriceTo.Value);
        }

        var orderBy = filters.SafeSort switch
        {
            "price_asc" => @"v.""Price"" ASC, v.""CreatedAt"" DESC",
            "price_desc" => @"v.""Price"" DESC, v.""CreatedAt"" DESC",
            "year_desc" => @"v.""Year"" DESC, v.""CreatedAt"" DESC",
            _ => @"v.""CreatedAt"" DESC, v.""Id"" DESC"
        };

        var sql = $"""
            SELECT v."Id" AS "Id",
                   v."PublicReference" AS "PublicReference",
                   v."Make" AS "Make",
                   v."Model" AS "Model",
                   v."Year" AS "Year",
                   v."Price" AS "Price",
                   v."Mileage" AS "Mileage",
                   (SELECT COUNT(*)::int FROM "VehicleImages" i WHERE i."VehicleId" = v."Id") AS "ImageCount",
                   (SELECT i."ImageUrl" FROM "VehicleImages" i
                    WHERE i."VehicleId" = v."Id" ORDER BY i."DisplayOrder", i."CreatedAt" LIMIT 1) AS "CoverImageUrl",
                   COUNT(*) OVER()::int AS "TotalCount"
            FROM "Vehicles" v
            {where}
            ORDER BY {orderBy}
            OFFSET @Offset LIMIT @Limit;
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        var rows = await connection.QueryAsync<ListRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var items = rows.Select(r => new PublicVehicleCardDto(
            r.Id, r.PublicReference, r.Make, r.Model, r.Year, r.Price, r.Mileage,
            r.ImageCount, r.CoverImageUrl)).ToList();

        return new PagedResult<PublicVehicleCardDto>(
            items, page, pageSize, rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<PublicVehicleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string vehicleSql = """
            SELECT v."Id" AS "Id",
                   v."PublicReference" AS "PublicReference",
                   v."Make" AS "Make",
                   v."Model" AS "Model",
                   v."Year" AS "Year",
                   v."Price" AS "Price",
                   v."Mileage" AS "Mileage",
                   v."Transmission" AS "Transmission",
                   v."FuelType" AS "FuelType",
                   v."Description" AS "Description"
            FROM "Vehicles" v
            WHERE v."Id" = @Id AND v."Status" = @Status;
            """;

        const string imagesSql = """
            SELECT "Id" AS "Id",
                   "ImageUrl" AS "ImageUrl",
                   "AltText" AS "AltText",
                   "DisplayOrder" AS "DisplayOrder"
            FROM "VehicleImages"
            WHERE "VehicleId" = @Id
            ORDER BY "DisplayOrder", "CreatedAt";
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var row = await connection.QueryFirstOrDefaultAsync<DetailRow>(
            new CommandDefinition(
                vehicleSql,
                new { Id = id, Status = (int)VehicleStatus.Available },
                cancellationToken: cancellationToken));
        if (row is null)
        {
            return null;
        }

        var images = await connection.QueryAsync<PublicVehicleImageDto>(
            new CommandDefinition(imagesSql, new { Id = id }, cancellationToken: cancellationToken));

        return new PublicVehicleDetailDto(
            row.Id, row.PublicReference, row.Make, row.Model, row.Year, row.Price, row.Mileage,
            row.Transmission, row.FuelType, row.Description, images.AsList(), null);
    }

    public async Task<PublicCatalogFacetsDto> GetFacetsAsync(
        string? make,
        CancellationToken cancellationToken = default)
    {
        const string makesSql = """
            SELECT "Make" AS "Value", COUNT(*)::int AS "Count"
            FROM "Vehicles"
            WHERE "Status" = @Status
            GROUP BY "Make"
            ORDER BY "Make";
            """;

        var modelsWhere = new StringBuilder(@"WHERE ""Status"" = @Status");
        var parameters = new DynamicParameters();
        parameters.Add("Status", (int)VehicleStatus.Available);
        if (!string.IsNullOrWhiteSpace(make))
        {
            modelsWhere.Append(@" AND ""Make"" ILIKE @Make");
            parameters.Add("Make", make.Trim());
        }

        var modelsSql = $"""
            SELECT "Model" AS "Value", COUNT(*)::int AS "Count"
            FROM "Vehicles"
            {modelsWhere}
            GROUP BY "Model"
            ORDER BY "Model";
            """;

        const string statsSql = """
            SELECT MIN("Year")::int AS "MinYear",
                   MAX("Year")::int AS "MaxYear",
                   MIN("Price") AS "MinPrice",
                   MAX("Price") AS "MaxPrice"
            FROM "Vehicles"
            WHERE "Status" = @Status;
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var multi = await connection.QueryMultipleAsync(
            new CommandDefinition($"{makesSql}\n{modelsSql}\n{statsSql}", parameters, cancellationToken: cancellationToken));

        var makes = (await multi.ReadAsync<PublicFacetOptionDto>()).AsList();
        var models = (await multi.ReadAsync<PublicFacetOptionDto>()).AsList();
        var stats = await multi.ReadFirstAsync<StatsRow>();

        return new PublicCatalogFacetsDto(
            makes, models, stats.MinYear, stats.MaxYear, stats.MinPrice, stats.MaxPrice);
    }

    private sealed record ListRow(
        Guid Id, string PublicReference, string Make, string Model, int Year, decimal Price,
        int Mileage, int ImageCount, string? CoverImageUrl, int TotalCount);

    private sealed record DetailRow(
        Guid Id, string PublicReference, string Make, string Model, int Year, decimal Price,
        int Mileage, string Transmission, string FuelType, string? Description);

    private sealed record StatsRow(int? MinYear, int? MaxYear, decimal? MinPrice, decimal? MaxPrice);
}
