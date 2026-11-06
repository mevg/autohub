using AutoHub.Application.Common.Exceptions;
using AutoHub.Domain.Enums;

namespace AutoHub.Application.Features.Vehicles.Commands.CreateVehicle;

public class CreateVehicleCommandHandler(
    IVehicleStore store,
    IVehicleQueries queries,
    IPublicReferenceGenerator publicReferences) : IRequestHandler<CreateVehicleCommand, Guid>
{
    public async Task<Guid> Handle(CreateVehicleCommand request, CancellationToken cancellationToken)
    {
        var vin = request.Vin.Trim().ToUpperInvariant();

        if (await queries.VinExistsAsync(vin, null, cancellationToken))
        {
            throw new DuplicateVinException(vin);
        }

        var vehicle = new Vehicle
        {
            Id = Guid.NewGuid(),
            Vin = vin,
            PublicReference = await publicReferences.NextAsync(cancellationToken),
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            Price = request.Price,
            Mileage = request.Mileage,
            Transmission = request.Transmission.Trim(),
            FuelType = request.FuelType.Trim(),
            Status = VehicleStatus.Available,
            Description = request.Description?.Trim(),
            CreatedBy = request.CreatedBy,
            CreatedAt = DateTime.UtcNow
        };

        store.Add(vehicle);
        await store.SaveChangesAsync(cancellationToken);

        return vehicle.Id;
    }
}
