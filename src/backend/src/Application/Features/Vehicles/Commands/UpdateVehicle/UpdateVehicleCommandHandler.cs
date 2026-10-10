using AutoHub.Application.Common.Exceptions;
using AutoHub.Domain.Enums;

namespace AutoHub.Application.Features.Vehicles.Commands.UpdateVehicle;

public class UpdateVehicleCommandHandler(
    IVehicleStore store,
    IVehicleQueries queries) : IRequestHandler<UpdateVehicleCommand, Unit>
{
    public async Task<Unit> Handle(UpdateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await store.GetForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vehículo '{request.Id}' no encontrado.");

        if (vehicle.Status == VehicleStatus.Archived)
        {
            throw new InvalidOperationException("No se puede editar un vehículo archivado.");
        }

        var vin = request.Vin.Trim().ToUpperInvariant();
        if (!string.Equals(vin, vehicle.Vin, StringComparison.Ordinal)
            && await queries.VinExistsAsync(vin, request.Id, cancellationToken))
        {
            throw new DuplicateVinException(vin);
        }

        vehicle.Vin = vin;
        vehicle.Make = request.Make.Trim();
        vehicle.Model = request.Model.Trim();
        vehicle.Year = request.Year;
        vehicle.Price = request.Price;
        vehicle.Mileage = request.Mileage;
        vehicle.Transmission = request.Transmission.Trim();
        vehicle.FuelType = request.FuelType.Trim();
        vehicle.Status = Enum.Parse<VehicleStatus>(request.Status, true);
        vehicle.Description = request.Description?.Trim();
        vehicle.UpdatedAt = DateTime.UtcNow;

        await store.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
