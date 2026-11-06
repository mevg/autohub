namespace AutoHub.Application.Features.PublicCatalog.Dtos;

public record PublicVehicleImageDto(
    Guid Id,
    string ImageUrl,
    string? AltText,
    int DisplayOrder
);

public record PublicVehicleCardDto(
    Guid Id,
    string PublicReference,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    int ImageCount,
    string? CoverImageUrl
);

public record PublicVehicleDetailDto(
    Guid Id,
    string PublicReference,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    string Transmission,
    string FuelType,
    string? Description,
    List<PublicVehicleImageDto> Images,
    string? WhatsAppUrl
);

public record PublicFacetOptionDto(string Value, int Count);

public record PublicCatalogFacetsDto(
    IReadOnlyList<PublicFacetOptionDto> Makes,
    IReadOnlyList<PublicFacetOptionDto> Models,
    int? MinYear,
    int? MaxYear,
    decimal? MinPrice,
    decimal? MaxPrice
);
