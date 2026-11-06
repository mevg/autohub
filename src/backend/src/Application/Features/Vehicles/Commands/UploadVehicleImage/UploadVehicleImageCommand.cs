using AutoHub.Application.Features.Vehicles.Dtos;

namespace AutoHub.Application.Features.Vehicles.Commands.UploadVehicleImage;

public record UploadVehicleImageCommand(
    Guid VehicleId,
    string ContentType,
    byte[] Data
) : IRequest<VehicleImageDto>;
