using AutoHub.Domain.Enums;

namespace AutoHub.Application.Features.Vehicles.Commands.ArchiveVehicle;

public record ArchiveVehicleCommand(Guid Id) : IRequest<Unit>;

public class ArchiveVehicleCommandHandler(IVehicleStore store) : IRequestHandler<ArchiveVehicleCommand, Unit>
{
    public async Task<Unit> Handle(ArchiveVehicleCommand request, CancellationToken cancellationToken)
    {
        var vehicle = await store.GetForUpdateAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vehículo '{request.Id}' no encontrado.");

        vehicle.Status = VehicleStatus.Archived;
        vehicle.UpdatedAt = DateTime.UtcNow;

        await store.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
