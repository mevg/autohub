namespace AutoHub.Application.Features.Settings.Dtos;

/// <summary>
/// URLs relativas al mismo origen (ej. "/branding/logo-abc123.png").
/// </summary>
public record BrandingDto(
    string AppName,
    string LogoUrl,
    string FaviconUrl,
    List<string> BannerUrls,
    string PrimaryLight,
    string Primary,
    string PrimaryDark
);
