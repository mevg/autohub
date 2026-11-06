using AutoHub.Application.Common.Dtos;
using AutoHub.Application.Features.Vehicles.Dtos;

namespace AutoHub.Application.Features.Vehicles.Queries.ListVehicles;

public record ListVehiclesQuery(VehicleFilters Filters) : IRequest<PagedResult<VehicleListItemDto>>;

public class ListVehiclesQueryHandler(IVehicleQueries queries) : IRequestHandler<ListVehiclesQuery, PagedResult<VehicleListItemDto>>
{
    public Task<PagedResult<VehicleListItemDto>> Handle(ListVehiclesQuery request, CancellationToken cancellationToken)
        => queries.ListAsync(request.Filters, cancellationToken);
}
