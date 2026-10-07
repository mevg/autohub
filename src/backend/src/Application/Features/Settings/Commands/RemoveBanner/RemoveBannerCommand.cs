using AutoHub.Application.Features.Settings.Dtos;

namespace AutoHub.Application.Features.Settings.Commands.RemoveBanner;

public record RemoveBannerCommand(int Index) : IRequest<BrandingDto>;

public class RemoveBannerCommandHandler(
    IBrandingStore store,
    IBrandAssetStorage storage) : IRequestHandler<RemoveBannerCommand, BrandingDto>
{
    public async Task<BrandingDto> Handle(RemoveBannerCommand request, CancellationToken cancellationToken)
    {
        var branding = await store.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Branding no inicializado.");

        if (request.Index < 0 || request.Index >= branding.BannerUrls.Count)
        {
            throw new InvalidOperationException("Índice de banner inválido.");
        }

        var url = branding.BannerUrls[request.Index];
        branding.BannerUrls.RemoveAt(request.Index);
        branding.UpdatedAt = DateTime.UtcNow;
        await store.SaveChangesAsync(cancellationToken);

        storage.Delete(url);

        return new BrandingDto(
            branding.AppName, branding.LogoUrl, branding.FaviconUrl,
            branding.BannerUrls, branding.PrimaryLight, branding.Primary, branding.PrimaryDark);
    }
}
