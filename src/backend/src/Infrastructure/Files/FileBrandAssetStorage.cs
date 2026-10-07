namespace AutoHub.Infrastructure.Files;

/// <summary>
/// Guarda assets de marca en wwwroot/branding con nombre hasheado.
/// La extensión se deriva del Content-Type validado, nunca del nombre original.
/// </summary>
public sealed class FileBrandAssetStorage(string webRootPath) : IBrandAssetStorage
{
    public const string Folder = "branding";

    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/svg+xml"] = ".svg",
        ["image/webp"] = ".webp",
        ["image/x-icon"] = ".ico",
        ["image/vnd.microsoft.icon"] = ".ico"
    };

    public async Task<string> SaveAsync(string slot, string contentType, byte[] data, CancellationToken cancellationToken = default)
    {
        if (!Extensions.TryGetValue(contentType, out var extension))
        {
            throw new InvalidOperationException("Tipo de imagen no permitido.");
        }

        var directory = Path.Combine(webRootPath, Folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{slot}-{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(directory, fileName), data, cancellationToken);

        return $"/{Folder}/{fileName}";
    }

    public void Delete(string url)
    {
        if (!url.StartsWith($"/{Folder}/", StringComparison.Ordinal))
        {
            return;
        }

        var fileName = Path.GetFileName(url);
        if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
        {
            return;
        }

        var path = Path.Combine(webRootPath, Folder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
