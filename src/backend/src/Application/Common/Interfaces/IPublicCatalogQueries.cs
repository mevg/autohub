using AutoHub.Application.Common.Dtos;
using AutoHub.Application.Features.PublicCatalog;
using AutoHub.Application.Features.PublicCatalog.Dtos;

namespace AutoHub.Application.Common.Interfaces;

public interface IPublicCatalogQueries
{
    Task<PagedResult<PublicVehicleCardDto>> ListAsync(
        PublicCatalogFilters filters,
        CancellationToken cancellationToken = default);

    Task<PublicVehicleDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PublicCatalogFacetsDto> GetFacetsAsync(string? make, CancellationToken cancellationToken = default);
}
