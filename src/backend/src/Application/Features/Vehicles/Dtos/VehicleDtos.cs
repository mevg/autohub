namespace AutoHub.Application.Features.Vehicles.Dtos;

public record VehicleImageDto(
    Guid Id,
    string ImageUrl,
    int DisplayOrder
);

public record VehicleListItemDto(
    Guid Id,
    string Vin,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    string Status,
    int ImageCount
);

public record VehicleDto(
    Guid Id,
    string Vin,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    string Transmission,
    string FuelType,
    string Status,
    string? Description,
    Guid CreatedBy,
    string CreatorEmail,
    List<VehicleImageDto> Images
);
