using AutoHub.Application.Features.Vehicles.Dtos;

namespace AutoHub.Application.Features.Vehicles.Queries.GetVehicleById;

public record GetVehicleByIdQuery(Guid Id) : IRequest<VehicleDto>;

public class GetVehicleByIdQueryHandler(IVehicleQueries queries) : IRequestHandler<GetVehicleByIdQuery, VehicleDto>
{
    public async Task<VehicleDto> Handle(GetVehicleByIdQuery request, CancellationToken cancellationToken)
        => await queries.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Vehículo '{request.Id}' no encontrado.");
}
