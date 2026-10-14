using System.Text;
using AutoHub.Domain.Enums;
using AutoHub.Application.Common.Dtos;
using AutoHub.Application.Features.Vehicles.Dtos;
using AutoHub.Application.Features.Vehicles.Queries.ListVehicles;

namespace AutoHub.Infrastructure.Persistence.Queries;

/// <summary>
/// Debe mantenerse en sync con <see cref="Configurations.VehicleConfiguration"/>
/// y <see cref="Configurations.VehicleImageConfiguration"/> (tablas en PascalCase).
/// </summary>
public sealed class DapperVehicleQueries(ISqlConnectionFactory connectionFactory) : IVehicleQueries
{
    public async Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string vehicleSql = """
            SELECT v."Id" AS "Id",
                   v."Vin" AS "Vin",
                   v."Make" AS "Make",
                   v."Model" AS "Model",
                   v."Year" AS "Year",
                   v."Price" AS "Price",
                   v."Mileage" AS "Mileage",
                   v."Transmission" AS "Transmission",
                   v."FuelType" AS "FuelType",
                   v."Status" AS "Status",
                   v."Description" AS "Description",
                   v."CreatedBy" AS "CreatedBy",
                   u."email" AS "CreatorEmail"
            FROM "Vehicles" v
            JOIN "users" u ON u."id" = v."CreatedBy"
            WHERE v."Id" = @Id;
            """;

        const string imagesSql = """
            SELECT "Id" AS "Id",
                   "ImageUrl" AS "ImageUrl",
                   "DisplayOrder" AS "DisplayOrder"
            FROM "VehicleImages"
            WHERE "VehicleId" = @Id
            ORDER BY "DisplayOrder";
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var row = await connection.QueryFirstOrDefaultAsync<VehicleRow?>(
            new CommandDefinition(vehicleSql, new { Id = id }, cancellationToken: cancellationToken));
        if (row is null)
        {
            return null;
        }

        var images = await connection.QueryAsync<VehicleImageDto>(
            new CommandDefinition(imagesSql, new { Id = id }, cancellationToken: cancellationToken));

        return new VehicleDto(
            row.Id, row.Vin, row.Make, row.Model, row.Year, row.Price, row.Mileage,
            row.Transmission, row.FuelType, ((VehicleStatus)row.Status).ToString(),
            row.Description, row.CreatedBy, row.CreatorEmail, images.AsList());
    }

    public async Task<PagedResult<VehicleListItemDto>> ListAsync(VehicleFilters filters, CancellationToken cancellationToken = default)
    {
        var page = filters.SafePage;
        var pageSize = filters.SafePageSize;

        var where = new StringBuilder("WHERE 1 = 1");
        var parameters = new DynamicParameters();
        parameters.Add("Offset", (page - 1) * pageSize);
        parameters.Add("Limit", pageSize);

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            where.Append(@" AND (v.""Make"" ILIKE @Search OR v.""Model"" ILIKE @Search OR v.""Vin"" ILIKE @Search)");
            parameters.Add("Search", $"%{filters.Search.Trim()}%");
        }

        if (!string.IsNullOrWhiteSpace(filters.Status) && Enum.TryParse<VehicleStatus>(filters.Status, true, out var status))
        {
            where.Append(@" AND v.""Status"" = @Status");
            parameters.Add("Status", (int)status);
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

        var sql = $"""
            SELECT v."Id" AS "Id",
                   v."Vin" AS "Vin",
                   v."Make" AS "Make",
                   v."Model" AS "Model",
                   v."Year" AS "Year",
                   v."Price" AS "Price",
                   v."Mileage" AS "Mileage",
                   v."Status" AS "Status",
                   (SELECT COUNT(*)::int FROM "VehicleImages" i WHERE i."VehicleId" = v."Id") AS "ImageCount",
                   (SELECT i."ImageUrl" FROM "VehicleImages" i
                    WHERE i."VehicleId" = v."Id" ORDER BY i."DisplayOrder" LIMIT 1) AS "CoverImageUrl",
                   COUNT(*) OVER()::int AS "TotalCount"
            FROM "Vehicles" v
            {where}
            ORDER BY v."CreatedAt" DESC
            OFFSET @Offset LIMIT @Limit;
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var rows = await connection.QueryAsync<ListRow>(
            new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));

        var items = rows.Select(r => new VehicleListItemDto(
            r.Id, r.Vin, r.Make, r.Model, r.Year, r.Price, r.Mileage,
            ((VehicleStatus)r.Status).ToString(), r.ImageCount, r.CoverImageUrl)).ToList();

        return new PagedResult<VehicleListItemDto>(items, page, pageSize, rows.FirstOrDefault()?.TotalCount ?? 0);
    }

    public async Task<bool> VinExistsAsync(string vin, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT EXISTS(
                SELECT 1 FROM "Vehicles"
                WHERE "Vin" = @Vin AND (@ExcludeId IS NULL OR "Id" <> @ExcludeId)
            );
            """;

        await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        return await connection.QueryFirstAsync<bool>(
            new CommandDefinition(sql, new { Vin = vin, ExcludeId = excludeId }, cancellationToken: cancellationToken));
    }

    private sealed record VehicleRow(
        Guid Id, string Vin, string Make, string Model, int Year, decimal Price, int Mileage,
        string Transmission, string FuelType, int Status, string? Description,
        Guid CreatedBy, string CreatorEmail);

    private sealed record ListRow(
        Guid Id, string Vin, string Make, string Model, int Year, decimal Price, int Mileage,
        int Status, int ImageCount, string? CoverImageUrl, int TotalCount);
}
