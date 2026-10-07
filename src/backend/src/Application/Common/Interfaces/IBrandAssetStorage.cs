namespace AutoHub.Application.Common.Interfaces;

public static class BrandAssetSlots
{
    public const string Logo = "logo";
    public const string Favicon = "favicon";
    public const string Banner = "banner";

    public static readonly string[] Allowed = [Logo, Favicon, Banner];
    public const int MaxBanners = 5;
}

/// <summary>
/// Guarda archivos de marca y devuelve la URL relativa pública.
/// </summary>
public interface IBrandAssetStorage
{
    Task<string> SaveAsync(string slot, string contentType, byte[] data, CancellationToken cancellationToken = default);
    void Delete(string url);
}
