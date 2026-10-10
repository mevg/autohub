namespace AutoHub.Application.Features.Vehicles.Queries.ListVehicles;

public record VehicleFilters(
    string? Search,
    string? Status,
    int? YearFrom,
    int? YearTo,
    decimal? PriceFrom,
    decimal? PriceTo,
    int Page,
    int PageSize
)
{
    public const int MaxPageSize = 50;

    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize is < 1 or > MaxPageSize ? MaxPageSize : PageSize;
}
