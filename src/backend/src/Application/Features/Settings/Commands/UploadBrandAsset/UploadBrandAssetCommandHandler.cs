using AutoHub.Application.Features.Settings.Dtos;

namespace AutoHub.Application.Features.Settings.Commands.UploadBrandAsset;

public class UploadBrandAssetCommandHandler(
    IBrandingStore store,
    IBrandAssetStorage storage) : IRequestHandler<UploadBrandAssetCommand, BrandingDto>
{
    public async Task<BrandingDto> Handle(UploadBrandAssetCommand request, CancellationToken cancellationToken)
    {
        var branding = await store.GetAsync(cancellationToken)
            ?? throw new InvalidOperationException("Branding no inicializado.");

        // Se guarda primero el archivo: si la BD falla después, queda un huérfano
        // en disco (inocuo); al revés se perdería la referencia al archivo anterior.
        var url = await storage.SaveAsync(request.Slot, request.ContentType, request.Data, cancellationToken);

        var previous = request.Slot switch
        {
            BrandAssetSlots.Logo => branding.LogoUrl,
            BrandAssetSlots.Favicon => branding.FaviconUrl,
            _ => string.Empty
        };

        if (request.Slot == BrandAssetSlots.Banner)
        {
            if (branding.BannerUrls.Count >= BrandAssetSlots.MaxBanners)
            {
                storage.Delete(url);
                throw new InvalidOperationException($"Máximo {BrandAssetSlots.MaxBanners} banners.");
            }
            branding.BannerUrls.Add(url);
        }
        else if (request.Slot == BrandAssetSlots.Logo)
        {
            branding.LogoUrl = url;
        }
        else
        {
            branding.FaviconUrl = url;
        }

        branding.UpdatedAt = DateTime.UtcNow;
        await store.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrEmpty(previous))
        {
            storage.Delete(previous);
        }

        return new BrandingDto(
            branding.AppName, branding.LogoUrl, branding.FaviconUrl,
            branding.BannerUrls, branding.PrimaryLight, branding.Primary, branding.PrimaryDark);
    }
}
