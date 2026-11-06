namespace AutoHub.Infrastructure.Files;

public sealed class LocalFileStorage(string webRootPath) : ILocalFileStorage
{
    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/png"] = ".png",
        ["image/jpeg"] = ".jpg",
        ["image/svg+xml"] = ".svg",
        ["image/webp"] = ".webp",
        ["image/x-icon"] = ".ico",
        ["image/vnd.microsoft.icon"] = ".ico"
    };

    public async Task<string> SaveAsync(string folder, string prefix, string contentType, byte[] data, CancellationToken cancellationToken = default)
    {
        if (!Extensions.TryGetValue(contentType, out var extension))
        {
            throw new InvalidOperationException("Tipo de imagen no permitido.");
        }

        var directory = Path.Combine(webRootPath, folder);
        Directory.CreateDirectory(directory);

        var fileName = $"{prefix}-{Guid.NewGuid():N}{extension}";
        await File.WriteAllBytesAsync(Path.Combine(directory, fileName), data, cancellationToken);

        return $"/{folder}/{fileName}";
    }

    public void Delete(string folder, string url)
    {
        if (!url.StartsWith($"/{folder}/", StringComparison.Ordinal))
        {
            return;
        }

        var fileName = Path.GetFileName(url);
        if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
        {
            return;
        }

        var path = Path.Combine(webRootPath, folder, fileName);
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
