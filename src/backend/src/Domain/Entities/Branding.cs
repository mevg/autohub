namespace AutoHub.Domain.Entities;

/// <summary>
/// Marca de la aplicación (fila única, Id = 1). Se edita desde el panel admin.
/// </summary>
public class Branding
{
    public int Id { get; set; } = 1;
    public string AppName { get; set; } = "AutoHub";
    public string LogoUrl { get; set; } = string.Empty;
    public string FaviconUrl { get; set; } = string.Empty;
    public List<string> BannerUrls { get; set; } = [];
    public string PrimaryLight { get; set; } = "#71E2DD";
    public string Primary { get; set; } = "#4DDBC4";
    public string PrimaryDark { get; set; } = "#45C5B0";
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
