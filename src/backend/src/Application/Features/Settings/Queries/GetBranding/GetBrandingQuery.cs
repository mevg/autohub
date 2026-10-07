using AutoHub.Application.Features.Settings.Dtos;

namespace AutoHub.Application.Features.Settings.Queries.GetBranding;

public record GetBrandingQuery : IRequest<BrandingDto>;

public class GetBrandingQueryHandler(IBrandingQueries brandingQueries) : IRequestHandler<GetBrandingQuery, BrandingDto>
{
    public async Task<BrandingDto> Handle(GetBrandingQuery request, CancellationToken cancellationToken)
    {
        var branding = await brandingQueries.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Branding no inicializado.");

        return new BrandingDto(
            branding.AppName,
            branding.LogoUrl,
            branding.FaviconUrl,
            branding.BannerUrls,
            branding.PrimaryLight,
            branding.Primary,
            branding.PrimaryDark
        );
    }
}
