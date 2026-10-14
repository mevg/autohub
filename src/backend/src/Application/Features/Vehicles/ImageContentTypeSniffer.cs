namespace AutoHub.Application.Features.Vehicles;

/// <summary>
/// Detecta el tipo de imagen por firma (magic bytes) cuando el navegador no
/// envía un Content-Type útil (p. ej. application/octet-stream en Linux).
/// </summary>
public static class ImageContentTypeSniffer
{
    public static string Sniff(string contentType, byte[] data)
    {
        if (!string.IsNullOrWhiteSpace(contentType)
            && !contentType.Equals("application/octet-stream", StringComparison.OrdinalIgnoreCase))
        {
            return contentType;
        }

        if (data.Length >= 12
            && data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46
            && data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50)
        {
            return "image/webp";
        }

        if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
        {
            return "image/jpeg";
        }

        if (data.Length >= 8
            && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47
            && data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
        {
            return "image/png";
        }

        return contentType;
    }
}
