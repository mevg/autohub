using AutoHub.Application.Common.Dtos;
using AutoHub.Application.Features.Vehicles.Dtos;
using AutoHub.Application.Features.Vehicles.Queries.ListVehicles;

namespace AutoHub.Application.Common.Interfaces;

/// <summary>
/// Lecturas de vehículos con Dapper (sin change tracking).
/// </summary>
public interface IVehicleQueries
{
    Task<VehicleDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResult<VehicleListItemDto>> ListAsync(VehicleFilters filters, CancellationToken cancellationToken = default);
    Task<bool> VinExistsAsync(string vin, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
