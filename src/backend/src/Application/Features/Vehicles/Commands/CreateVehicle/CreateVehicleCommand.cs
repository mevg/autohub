namespace AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;

/// <param name="CreatedBy">Lo fija el endpoint desde el claim del token, nunca el cliente.</param>
public record CreateVehicleCommand(
    string Vin,
    string Make,
    string Model,
    int Year,
    decimal Price,
    int Mileage,
    string Transmission,
    string FuelType,
    string? Description,
    Guid CreatedBy
) : IRequest<Guid>;
