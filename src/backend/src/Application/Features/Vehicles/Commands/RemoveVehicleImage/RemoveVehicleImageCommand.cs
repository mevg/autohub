namespace AutoHub.Application.Features.Vehicles.Commands.RemoveVehicleImage;

public record RemoveVehicleImageCommand(Guid VehicleId, Guid ImageId) : IRequest<Unit>;

public class RemoveVehicleImageCommandHandler(
    IVehicleStore store,
    ILocalFileStorage files) : IRequestHandler<RemoveVehicleImageCommand, Unit>
{
    public async Task<Unit> Handle(RemoveVehicleImageCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await store.GetForUpdateAsync(request.VehicleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Vehículo '{request.VehicleId}' no encontrado.");

        var image = vehicle.Images.FirstOrDefault(i => i.Id == request.ImageId)
            ?? throw new KeyNotFoundException($"Imagen '{request.ImageId}' no encontrada.");

        vehicle.Images.Remove(image);

        var order = 0;
        foreach (var remaining in vehicle.Images.OrderBy(i => i.DisplayOrder))
        {
            remaining.DisplayOrder = order++;
        }

        vehicle.UpdatedAt = DateTime.UtcNow;
        await store.SaveChangesAsync(cancellationToken);

        files.Delete(VehicleImageRules.StorageFolder, image.ImageUrl);

        return Unit.Value;
    }
}
