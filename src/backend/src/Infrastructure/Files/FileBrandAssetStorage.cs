namespace AutoHub.Infrastructure.Files;

/// <summary>
/// Guarda assets de marca en wwwroot/branding con nombre hasheado.
/// </summary>
public sealed class FileBrandAssetStorage(ILocalFileStorage files) : IBrandAssetStorage
{
    public const string Folder = "branding";

    public Task<string> SaveAsync(string slot, string contentType, byte[] data, CancellationToken cancellationToken = default)
        => files.SaveAsync(Folder, slot, contentType, data, cancellationToken);

    public void Delete(string url)
        => files.Delete(Folder, url);
}
