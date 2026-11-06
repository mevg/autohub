namespace AutoHub.Application.Features.PublicCatalog;

public record PublicCatalogFilters(
    string? Search,
    string? Make,
    string? Model,
    int? YearFrom,
    int? YearTo,
    decimal? PriceFrom,
    decimal? PriceTo,
    string? Sort,
    int Page,
    int PageSize
)
{
    public const int MaxPageSize = 24;
    public const int MaxSearchLength = 100;
    public const int MaxFacetValueLength = 50;

    public int SafePage => Page < 1 ? 1 : Page;
    public int SafePageSize => PageSize is < 1 or > MaxPageSize ? MaxPageSize : PageSize;

    public string SafeSort => Sort?.Trim().ToLowerInvariant() switch
    {
        "price_asc" => "price_asc",
        "price_desc" => "price_desc",
        "year_desc" => "year_desc",
        _ => "newest"
    };

    public string? SafeSearch => Clean(Search, MaxSearchLength);
    public string? SafeMake => Clean(Make, MaxFacetValueLength);
    public string? SafeModel => Clean(Model, MaxFacetValueLength);

    private static string? Clean(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length > maxLength ? trimmed[..maxLength] : trimmed;
    }
}
