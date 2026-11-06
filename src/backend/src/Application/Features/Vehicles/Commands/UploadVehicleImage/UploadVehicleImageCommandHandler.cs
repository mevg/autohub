using AutoHub.Application.Features.Vehicles.Dtos;
using AutoHub.Domain.Entities;

namespace AutoHub.Application.Features.Vehicles.Commands.UploadVehicleImage;

public class UploadVehicleImageCommandHandler(
    IVehicleStore store,
    ILocalFileStorage files) : IRequestHandler<UploadVehicleImageCommand, VehicleImageDto>
{
    public async Task<VehicleImageDto> Handle(UploadVehicleImageCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await store.GetForUpdateAsync(request.VehicleId, cancellationToken)
            ?? throw new KeyNotFoundException($"Vehículo '{request.VehicleId}' no encontrado.");

        if (vehicle.Images.Count >= VehicleImageRules.MaxPerVehicle)
        {
            throw new InvalidOperationException($"Máximo {VehicleImageRules.MaxPerVehicle} imágenes por vehículo.");
        }

        var url = await files.SaveAsync(
            VehicleImageRules.StorageFolder, request.VehicleId.ToString("N"),
            request.ContentType, request.Data, cancellationToken);

        var image = new VehicleImage
        {
            Id = Guid.NewGuid(),
            VehicleId = vehicle.Id,
            ImageUrl = url,
            DisplayOrder = vehicle.Images.Count == 0 ? 0 : vehicle.Images.Max(i => i.DisplayOrder) + 1,
            CreatedAt = DateTime.UtcNow
        };

        vehicle.Images.Add(image);
        store.AddImage(image);
        vehicle.UpdatedAt = DateTime.UtcNow;
        await store.SaveChangesAsync(cancellationToken);

        return new VehicleImageDto(image.Id, image.ImageUrl, image.DisplayOrder);
    }
}
