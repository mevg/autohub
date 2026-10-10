namespace AutoHub.Application.Features.Vehicles.Commands.UpdateVehicle;

public record UpdateVehicleCommand(
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
    string? Description
) : IRequest<Unit>;
